using EthereumExchangeWallet.Api.Common;
using EthereumExchangeWallet.Api.Data.Entities;
using EthereumExchangeWallet.Api.Data.Repositories;
using EthereumExchangeWallet.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EthereumExchangeWallet.Api.Controllers.Dtos;

namespace EthereumExchangeWallet.Api.Controllers
{
    [Route("api/assets")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IRepository<Asset> _repository;
        private readonly IAssetService _assetService;

        public AssetsController(IRepository<Asset> repository, IAssetService assetService)
        {
            _repository = repository;
            _assetService = assetService;
        }

        [HttpGet]
        public async Task<IEnumerable<AssetDto>> GetAssetsAsync()
        {
            return (await _repository.GetListAsync()).Select(asset => asset.AsDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetDto>> GetAssetByIdAsync(int id)
        {
            var asset = await _repository.GetItemAsync(id);
            if (asset == null) return NotFound();

            return asset.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<AssetDto>> CreateAssetAsync([FromBody] CreateUpdateAssetDto assetDto)
        {
            Asset asset = new()
            {
                Name = assetDto.Name
            };

            var result = await _repository.CreateAsync(asset);

            return CreatedAtAction(nameof(GetAssetByIdAsync), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAssetAsync(int id, [FromBody] CreateUpdateAssetDto assetDto)
        {
            var existingAsset = await _repository.GetItemAsync(id);
            if (existingAsset == null) return NotFound();

            existingAsset.Name = assetDto.Name;

            await _repository.UpdateAsync(existingAsset);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAssetAsync(int id)
        {
            var existingAsset = await _repository.GetItemAsync(id);
            if (existingAsset == null) return NotFound();

            await _repository.DeleteAsync(existingAsset);

            return NoContent();
        }

        [HttpPost("{id}/smart-contract-address")]
        public async Task<ActionResult> CreateAssetSmartContractAddress(int id)
        {
            var smartContractAdress = await _assetService.TryCreateAssetSmartContractForwarderAsync(id);

            if (smartContractAdress == false) return NotFound();

            // TODO: change this
            return NoContent();
        }

        [HttpGet("{asset}/{address}")]
        public async Task<ActionResult> GetAssetBalance(int asset, string address)
        {
            var balance = await _assetService.GetAssetBalance(address, asset);

            return Ok(balance);
            // TODO: hendlat osale caseove
        }
    }
}
