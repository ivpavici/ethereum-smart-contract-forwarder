using EthereumExchangeWallet.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EthereumExchangeWallet.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>().HasData(
                    new Asset
                    {
                        Id = 1,
                        Name = "Ethereum"
                    }
                );

            modelBuilder.Entity<User>().HasData(
                   new User
                   {
                       Id = 1,
                       Username = "TestUser",
                   }
               );
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DepositAddress> DepositAddresses { get; set; }
    }
}
