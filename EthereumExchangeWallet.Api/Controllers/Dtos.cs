using EthereumExchangeWallet.Api.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EthereumExchangeWallet.Api.Controllers
{
    public class Dtos
    {
        public record AssetDto(int Id, string Name);
        public record UserDto(string Username, IEnumerable<DepositAddress> DepositAddresses);
        public record UserDepositAssetDto(int AssetId);
        public record CreateUpdateAssetDto([Required] string Name);
        public record CreateUpdateUserDto(string Username);
        public record DepositDto(decimal Amount, int AssetId);
    }
}
