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
        public Web3 Web3 { get; }

        public NethereumService(IConfiguration config, ILogger<NethereumService> logger)
        {
            _config = config;
            _logger = logger;
            // TODO: should be in config / secret
            var privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

            var chainIdString = _config.GetValue<string>("ChainId");
            var chainId = BigInteger.Parse(chainIdString);

            var account = new Account(privateKey, chainId);
            Web3 = new Web3(account);
        }

        public async Task<string> DeployDefaultEthContractForwarderAddress()
        {
            var destinationAddress = _config.GetValue<string>("EthHotWalletAddress");

            //Deploying first the default forwarder (template for all clones)
            string defaultForwaderContractAddress;
            try
            {
                var defaultForwarderDeploymentReceipt = await ForwarderService.DeployContractAndWaitForReceiptAsync(Web3, new ForwarderDeployment());
                defaultForwaderContractAddress = defaultForwarderDeploymentReceipt.ContractAddress;
                var defaultForwarderService = new ForwarderService(Web3, defaultForwaderContractAddress);
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
            var factoryDeploymentReceipt = await ForwarderFactoryService.DeployContractAndWaitForReceiptAsync(Web3, new ForwarderFactoryDeployment());

            return factoryDeploymentReceipt.ContractAddress;
        }

        public string CalculateDepositAddress(int userId, string factoryAddress, string defaultForwaderContractAddress)
        {
            var salt = BigInteger.Parse(userId.ToString());
            var saltHex = new IntTypeEncoder().Encode(salt).ToHex();

            return ContractUtils.CalcualteCreate2AddressMinimalProxy(factoryAddress, saltHex, defaultForwaderContractAddress);
        }

        public static int calls = 0;


        public async Task Deposit(decimal amount, int userId, string addressToDeposit, string forwaderContractAddress, string factoryAddress)
        {
            // These first two lines of code are a simulation of deposit through, for example, Metamask

            // Let's tranfer some ether, with some extra gas to allow forwarding if the smart contract is deployed (UX problem)
            var estimatedGas = await Web3.Eth.GetEtherTransferService().EstimateGasAsync(addressToDeposit, amount);
            var transferEtherReceipt = await Web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(addressToDeposit, amount, null, estimatedGas);

            // This whole code below should be done inside some scheduler application that monitors the blockchain addresses for deposits.
            // It will perform the flush to our hot wallet just for the first time. After that, each deoposit to users address is automatically 
            // forwarded to the hot wallet through the smart contract.
            calls++;
            
            // Create the clone with the salt to match the address
            var factoryService = new ForwarderFactoryService(Web3, factoryAddress);
            var salt = BigInteger.Parse(userId.ToString());

            if (calls == 1)
            {
                var txnReceipt = await factoryService.CloneForwarderRequestAndWaitForReceiptAsync(forwaderContractAddress, salt);

                // create a service to for cloned forwarder
                var clonedForwarderService = new ForwarderService(Web3, addressToDeposit);

                // Using flush directly in the cloned contract
                // call flush to get all the ether transferred to destination address 
                var flushReceipt = await clonedForwarderService.FlushRequestAndWaitForReceiptAsync();
            }

        }

        public async Task<decimal> GetBalance(string address, int asset)
        {
            BigInteger balance;

            if (asset == 1) // Eth
            {
                try
                {
                    balance = await Web3.Eth.GetBalance.SendRequestAsync(address);
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

                var balanceHandler = Web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

                // TODO: contractAddress je vjerojatno adresa tokena na blockchainu
                balance = await balanceHandler.QueryAsync<BigInteger>("contractAddress", balanceOfFunctionMessage);
            }

            return Web3.Convert.FromWei(balance);
        }
    }
}
