using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public interface IAssetService
    {
        Task<bool> TryCreateAssetSmartContractForwarderAsync(int assetId);
        Task<decimal> GetAssetBalance(string address, int assetId);
    }
}
