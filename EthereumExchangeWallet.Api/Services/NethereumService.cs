using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public class NethereumService : INethereumService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<NethereumService> _logger;
        private Web3 _web3;

        public Web3 GetWeb3()
        {
            if (_web3 == null)
            {
                var privateKey = _config.GetValue<string>("EthPrivateKey");
                var chainIdString = _config.GetValue<string>("ChainId");
                var chainId = BigInteger.Parse(chainIdString);

                var account = new Account(privateKey, chainId);
                var ethNetwork = _config.GetValue<string>("EthNetwork");

                _web3 = new Web3(account, ethNetwork);
            }

            return _web3;
        }

        public NethereumService(IConfiguration config, ILogger<NethereumService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> DeployDefaultEthContractForwarderAddress()
        {
            var destinationAddress = _config.GetValue<string>("EthHotWalletAddress");

            //Deploying first the default forwarder (template for all clones)
            string defaultForwaderContractAddress;
            try
            {
                var defaultForwarderDeploymentReceipt = await ForwarderService.DeployContractAndWaitForReceiptAsync(GetWeb3(), new ForwarderDeployment());
                defaultForwaderContractAddress = defaultForwarderDeploymentReceipt.ContractAddress;
                var defaultForwarderService = new ForwarderService(GetWeb3(), defaultForwaderContractAddress);
                // initialiasing with the destination address
                await defaultForwarderService.ChangeDestinationRequestAndWaitForReceiptAsync(destinationAddress);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot connect to Ethereum blockchain");
                throw;
            }
            
            return defaultForwaderContractAddress;
        }

        public async Task<string> DeployDefaultEthContractFactory()
        {
            var factoryDeploymentReceipt = await ForwarderFactoryService.DeployContractAndWaitForReceiptAsync(GetWeb3(), new ForwarderFactoryDeployment());

            return factoryDeploymentReceipt.ContractAddress;
        }

        public string CalculateDepositAddress(int userId, string factoryAddress, string defaultForwaderContractAddress)
        {
            var salt = BigInteger.Parse(userId.ToString());
            var saltHex = new IntTypeEncoder().Encode(salt).ToHex();

            return CalculateCreate2AddressMinimalProxy(factoryAddress, saltHex, defaultForwaderContractAddress);
        }

        public async Task Deposit(decimal amount, string addressToDeposit)
        {
            // a simulation of deposit with, for example, Metamask

            // Let's tranfer some ether, with some extra gas to allow forwarding if the smart contract is deployed (UX problem)
            var estimatedGas = await GetWeb3().Eth.GetEtherTransferService().EstimateGasAsync(addressToDeposit, amount);

            var transferEtherReceipt = await GetWeb3().Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(addressToDeposit, amount, null, estimatedGas);
        }

        public async Task FlushEthereum(int userId, string addressToDeposit, string forwaderContractAddress, string factoryAddress)
        {
            // Create the clone with the salt to match the address
            var factoryService = new ForwarderFactoryService(GetWeb3(), factoryAddress);
            
            var salt = BigInteger.Parse(userId.ToString());
            var txnReceipt = await factoryService.CloneForwarderRequestAndWaitForReceiptAsync(forwaderContractAddress, salt);

            // create a service for cloned forwarder
            var clonedForwarderService = new ForwarderService(GetWeb3(), addressToDeposit);

            // Using flush directly in the cloned contract
            // call flush to get all the ether transferred to destination address 
            var flushAllReceipt = await clonedForwarderService.FlushRequestAndWaitForReceiptAsync();
        }

        public async Task<decimal> GetBalance(string address, int asset)
        {
            BigInteger balance;

            if (asset == 1) // Eth
            {
                try
                {
                    balance = await GetWeb3().Eth.GetBalance.SendRequestAsync(address);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot connect to Ethereum blockchain");
                    throw;
                }
            }
            else
            {
                // if asset is token
                var balanceOfFunctionMessage = new BalanceOfFunction()
                {
                    Owner = "address", // TODO: ovde ide adresa deployanog smart contract forwardera ili nase hot wallet adrese
                };

                var balanceHandler = GetWeb3().Eth.GetContractQueryHandler<BalanceOfFunction>();

                // TODO: contractAddress je vjerojatno adresa tokena na blockchainu
                balance = await balanceHandler.QueryAsync<BigInteger>("contractAddress", balanceOfFunctionMessage);
            }

            return Web3.Convert.FromWei(balance);
        }

        //this code is also from Nethereum Util, but it didn't work in latest version
        private static string CalculateCreate2AddressMinimalProxy(string address, string saltHex, string deploymentAddress)
        {
            if (string.IsNullOrEmpty(deploymentAddress))
            {
                throw new System.ArgumentException($"'{nameof(deploymentAddress)}' cannot be null or empty.", nameof(deploymentAddress));
            }

            var bytecode = "3d602d80600a3d3981f3363d3d373d3d3d363d73" + deploymentAddress.RemoveHexPrefix() + "5af43d82803e903d91602b57fd5bf3";
            return CalculateCreate2Address(address, saltHex, bytecode);
        }

        //this code is also from Nethereum Util, but it didn't work in latest version
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
