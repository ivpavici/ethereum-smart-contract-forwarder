using Nethereum.Web3;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public interface INethereumService
    {
        Task<string> DeployDefaultEthContractForwarderAddress();
        Task<string> DeployDefaultEthContractFactory();
        string CalculateDepositAddress(int userId, string factoryAddress, string forwaderContractAddress);
        Task Deposit(decimal amount, string addressToDeposit);
        Task FlushEthereum(int userId, string addressToDeposit, string forwaderContractAddress, string factoryAddress);
        Task<decimal> GetBalance(string address, int asset);
    }
}