using System.Collections.Generic;

namespace EthereumExchangeWallet.Api.Data.Entities
{
    public class User
    {
        public int Id { get; init; }
        public string Username { get; set; }
        public IEnumerable<DepositAddress> DepositAddresses { get; set; }
    }
}