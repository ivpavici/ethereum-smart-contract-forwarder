using EthereumExchangeWallet.Api.Data.Entities;
using EthereumExchangeWallet.Api.Data.Repositories;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public class AssetService : IAssetService
    {
        private readonly IRepository<DepositAddress> _depositAddressRepository;
        private readonly IRepository<Asset> _assetRepository;
        private readonly INethereumService _nethereumService;

        public AssetService(IRepository<DepositAddress> depositAddressRepository, IRepository<Asset> assetRepository, INethereumService nethereumService)
        {
            _depositAddressRepository = depositAddressRepository;
            _assetRepository = assetRepository;
            _nethereumService = nethereumService;
        }

        // Should be called just by admin
        public async Task<bool> TryCreateAssetSmartContractForwarderAsync(int assetId)
        {
            var asset = await _assetRepository.GetItemAsync(assetId);

            var defaultForwaderContractAddress = await _nethereumService.DeployDefaultEthContractForwarderAddress();
            DepositAddress forwarderAddress = new()
            {
                Address = defaultForwaderContractAddress,
                Asset = asset,
                IsContractForwarderAddress = true
            };

            await _depositAddressRepository.CreateAsync(forwarderAddress);

            var contractFactoryAddress = await _nethereumService.DeployDefaultEthContractFactory();
            DepositAddress factoryAddress = new()
            {
                Address = contractFactoryAddress,
                Asset = asset,
                IsContractFactoryAddress = true
            };

            await _depositAddressRepository.CreateAsync(factoryAddress);

            return true;
        }

        public async Task<decimal> GetAssetBalance(string address, int assetId)
        {
            return await _nethereumService.GetBalance(address, assetId);
        }
    }
}
