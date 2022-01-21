using EthereumExchangeWallet.Api.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Data.Repositories
{
    public class InMemAssetsRepository : IRepository<Asset>
    {
        private readonly List<Asset> assets = new()
        {
            new Asset { Id = 1, Name = "Ethereum" },
            new Asset { Id = 2, Name = "TestToken" }
        };

        public async Task<IEnumerable<Asset>> GetListAsync()
        {
            return await Task.FromResult(assets);
        }

        public async Task<Asset> GetItemAsync(int id)
        {
            return await Task.FromResult(assets.Where(item => item.Id == id).SingleOrDefault());
        }

        public async Task<Asset> CreateAsync(Asset asset)
        {
            assets.Add(asset);

            return await Task.FromResult(asset);
        }

        public async Task UpdateAsync(Asset asset)
        {
            var index = assets.FindIndex(existingAsset => existingAsset.Id == asset.Id);
            assets[index] = asset;

            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Asset asset)
        {
            var index = assets.FindIndex(existingAsset => existingAsset.Id == asset.Id);
            assets.RemoveAt(index);

            await Task.CompletedTask;
        }
    }
}
