using Nethereum.Web3;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public interface INethereumService
    {
        Web3 Web3 { get; }
        Task<string> DeployDefaultEthContractForwarderAddress();
        Task<string> DeployDefaultEthContractFactory();
        string CalculateDepositAddress(int userId, string factoryAddress, string forwaderContractAddress);
        Task Deposit(decimal amount, int userId, string addressToDeposit, string forwaderContractAddress, string factoryAddress);
        Task FlushEthereum(int userId, string addressToDeposit, string forwaderContractAddress, string factoryAddress);
        Task<decimal> GetBalance(string address, int asset);
    }
}