using EthereumExchangeWallet.Api.Controllers;
using EthereumExchangeWallet.Api.Data.Entities;
using EthereumExchangeWallet.Api.Data.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static EthereumExchangeWallet.Api.Controllers.Dtos;

namespace EthereumExchangeWallet.Test
{
    public class AssetsControllerTest
    {
        private readonly Mock<IRepository<Asset>> _assetRepositoryStub = new();
        private readonly Mock<IRepository<Asset>> _assetServiceStub = new();

        [Fact]
        public async Task GetAssetByIdAync_WithUnexistingItem_ReturnsNotFound()
        {
            // Arrange
            _assetRepositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<int>()))
                .ReturnsAsync((Asset)null);

            var controller = new AssetsController(_assetRepositoryStub.Object, null);

            // Act
            var result = await controller.GetAssetByIdAsync(1);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAssetByIdAync_WithExistingItem_ReturnsExpectedAsset()
        {
            // Arrange
            var expectedItem = CreateAssets().First();
            _assetRepositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedItem);

            var controller = new AssetsController(_assetRepositoryStub.Object, null);

            // Act
            var result = await controller.GetAssetByIdAsync(1);

            // Assert
            result.Value.Should().BeEquivalentTo(expectedItem);
        }

        [Fact]
        public async Task GetAssetsAsync_WithExistingItems_ReturnsAllAssets()
        {
            // Arrange
            var expectedAssets = CreateAssets();
            _assetRepositoryStub.Setup(repo => repo.GetListAsync())
                .ReturnsAsync(expectedAssets);

            var controller = new AssetsController(_assetRepositoryStub.Object, null);

            // Act
            var actualAssets = await controller.GetAssetsAsync();

            // Assert
            actualAssets.Should().BeEquivalentTo(expectedAssets);
        }

        [Fact]
        public async Task CreateAssetAsync_WithAssetToCreate_ReturnsCreatedAsset()
        {
            // Arrange
            var assetToCreate = new CreateUpdateAssetDto("Ethereum");

            var expected = CreateAssets().First();
            _assetRepositoryStub.Setup(repo => repo.CreateAsync(It.IsAny<Asset>()))
                .ReturnsAsync(expected);

            var controller = new AssetsController(_assetRepositoryStub.Object, null);

            // Act
            var result = await controller.CreateAssetAsync(assetToCreate);

            // Assert
            var createdItem = (result.Result as CreatedAtActionResult).Value;
            assetToCreate.Should().BeEquivalentTo(createdItem, options => options.ExcludingMissingMembers());
        }

        [Fact]
        public async Task UpdateAssetAsync_WitExistingAsset_ReturnsNoContent()
        {
            // Arrange
            var asset = CreateAssets().First();
            _assetRepositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<int>()))
                .ReturnsAsync(asset);

            var controller = new AssetsController(_assetRepositoryStub.Object, null);

            var assetId = asset.Id;
            var assetDto = new CreateUpdateAssetDto("Eth");

            // Act
            var result = await controller.UpdateAssetAsync(assetId, assetDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAssetAsync_WitExistingAsset_ReturnsNoContent()
        {
            // Arrange
            var asset = CreateAssets().First();
            _assetRepositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<int>()))
                .ReturnsAsync(asset);

            var controller = new AssetsController(_assetRepositoryStub.Object, null);

            // Act
            var result = await controller.DeleteAssetAsync(asset.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

            private static IEnumerable<Asset> CreateAssets()
        {
            return new List<Asset> {
               new Asset { Id = 1, Name = "Ethereum" },
               new Asset { Id = 2, Name = "TestToken 1" },
               new Asset { Id = 2, Name = "TestToken 2" },
            };
        }
    }
}
