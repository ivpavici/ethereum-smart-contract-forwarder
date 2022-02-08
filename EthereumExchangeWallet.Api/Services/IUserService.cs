using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public interface IUserService
    {
        Task<bool> TryAddDepositAddress(int userId, int assetId);
        Task<bool> TryDepositToAddress(decimal amount, int userId, int assetId);
        Task<bool> TryFlushEthereumToHotWallet(int userId);
    }
}
