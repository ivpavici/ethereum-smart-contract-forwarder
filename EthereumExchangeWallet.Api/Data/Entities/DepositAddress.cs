namespace EthereumExchangeWallet.Api.Data.Entities
{
    public class DepositAddress
    {
        public int Id { get; init; }
        public string Address { get; set; }
        public Asset Asset { get; set; }
        public User User { get; set; }
        public bool IsContractFactoryAddress { get; set; }
        public bool IsContractForwarderAddress { get; set; }
    }
}