using EthereumExchangeWallet.Api.Data.Entities;
using static EthereumExchangeWallet.Api.Controllers.Dtos;

namespace EthereumExchangeWallet.Api.Common
{
    public static class Extensions
    {
        public static AssetDto AsDto(this Asset asset)
        {
            return new AssetDto(asset.Id, asset.Name);
        }

        public static UserDto AsDto(this User user)
        {
            return new UserDto(user.Username, user.DepositAddresses);
        }
    }
}
