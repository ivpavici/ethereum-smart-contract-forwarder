using Microsoft.Extensions.Configuration;
using Nethereum.ABI.Encoders;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Nethereum.WalletForwarder.Contracts.Forwarder;
using Nethereum.WalletForwarder.Contracts.Forwarder.ContractDefinition;
using Nethereum.WalletForwarder.Contracts.ForwarderFactory;
using Nethereum.WalletForwarder.Contracts.ForwarderFactory.ContractDefinition;
using Nethereum.WalletForwarder.Contracts.IERC20.ContractDefinition;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public class NethereumService : INethereumService
    {
        private readonly IConfiguration _config;
        public Web3 Web3 { get; }

        public NethereumService(IConfiguration config)
        {
            _config = config;
            // TODO: should be in config / secret
            var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var account = new Account(privateKey, 444444444500);
            Web3 = new Web3(account);  
        }

        public async Task<string> DeployDefaultEthContractForwarderAddress()
        {
            var destinationAddress = _config.GetValue<string>("EthHotWalletAddress");

            // TODO: ubacit try catch ako ne radi blockchain veza

            //Deploying first the default forwarder (template for all clones)
            var defaultForwarderDeploymentReceipt = await ForwarderService.DeployContractAndWaitForReceiptAsync(Web3, new ForwarderDeployment());
            var defaultForwaderContractAddress = defaultForwarderDeploymentReceipt.ContractAddress;
            var defaultForwarderService = new ForwarderService(Web3, defaultForwaderContractAddress);
            //initialiasing with the destination address
            await defaultForwarderService.ChangeDestinationRequestAndWaitForReceiptAsync(destinationAddress);

            return defaultForwaderContractAddress;
        }

        public async Task<string> DeployDefaultEthContractFactory()
        {
            var factoryDeploymentReceipt = await ForwarderFactoryService.DeployContractAndWaitForReceiptAsync(Web3, new ForwarderFactoryDeployment());

            return factoryDeploymentReceipt.ContractAddress;
        }

        public string CalculateDepositAddress(int userId, string factoryAddress, string defaultForwaderContractAddress)
        {
            var salt = BigInteger.Parse(userId.ToString());
            var saltHex = new IntTypeEncoder().Encode(salt).ToHex();

            return CalculateCreate2AddressMinimalProxy(factoryAddress, saltHex, defaultForwaderContractAddress);
        }

        public static int calls = 0;

        public async Task Deposit(decimal amount, int userId, string addressToDeposit, string forwaderContractAddress, string factoryAddress)
        {
#region OVO BI TRIBA METAMASK RADIT
            //Let's tranfer some ether, with some extra gas to allow forwarding if the smart contract is deployed (UX problem)
            var estimatedGas = await Web3.Eth.GetEtherTransferService().EstimateGasAsync(addressToDeposit, amount);
            var transferEtherReceipt = await Web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(addressToDeposit, amount, null, estimatedGas);
            #endregion

            calls++;

            //Create the clone with the salt to match the address
            var factoryService = new ForwarderFactoryService(Web3, factoryAddress);
            var salt = BigInteger.Parse(userId.ToString());

            // TODO: store no of calls in db
            if (calls == 1)
            {
                var txnReceipt = await factoryService.CloneForwarderRequestAndWaitForReceiptAsync(forwaderContractAddress, salt);

                //create a service to for cloned forwarder
                var clonedForwarderService = new ForwarderService(Web3, addressToDeposit);

                //Using flush directly in the cloned contract
                //call flush to get all the ether transferred to destination address 
                var flushReceipt = await clonedForwarderService.FlushRequestAndWaitForReceiptAsync();
            }

        }

        public async Task<decimal> GetBalance(string address, int asset)
        {
            BigInteger balance;

            if (asset == 1) // Eth
                balance = await Web3.Eth.GetBalance.SendRequestAsync(address);
            else
            {
                // if asset is token
                var balanceOfFunctionMessage = new BalanceOfFunction()
                {
                    Owner = "address", // TODO: ovde ide adresa deployanog smart contract forwardera ili nase hot wallet adrese
                };

                var balanceHandler = Web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

                // TODO: contractAddress je vjerojatno adresa tokena na blockchainu
                balance = await balanceHandler.QueryAsync<BigInteger>("contractAddress", balanceOfFunctionMessage);
            }

            return Web3.Convert.FromWei(balance);
        }

        //extracted from latest Nethereum Util
        private static string CalculateCreate2AddressMinimalProxy(string address, string saltHex, string deploymentAddress)
        {
            if (string.IsNullOrEmpty(deploymentAddress))
            {
                throw new System.ArgumentException($"'{nameof(deploymentAddress)}' cannot be null or empty.", nameof(deploymentAddress));
            }

            var bytecode = "3d602d80600a3d3981f3363d3d373d3d3d363d73" + deploymentAddress.RemoveHexPrefix() + "5af43d82803e903d91602b57fd5bf3";
            return CalculateCreate2Address(address, saltHex, bytecode);
        }

        //extracted from latest Nethereum Util
        private static string CalculateCreate2Address(string address, string saltHex, string byteCodeHex)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new System.ArgumentException($"'{nameof(address)}' cannot be null or empty.", nameof(address));
            }

            if (string.IsNullOrEmpty(saltHex))
            {
                throw new System.ArgumentException($"'{nameof(saltHex)}' cannot be null or empty.", nameof(saltHex));
            }

            if (saltHex.EnsureHexPrefix().Length != 66)
            {
                throw new System.ArgumentException($"'{nameof(saltHex)}' needs to be 32 bytes", nameof(saltHex));
            }

            var sha3 = new Sha3Keccack();
            return sha3.CalculateHashFromHex("0xff", address, saltHex, sha3.CalculateHashFromHex(byteCodeHex)).Substring(24).ConvertToEthereumChecksumAddress();
        }
    }
}
