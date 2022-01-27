using EthereumExchangeWallet.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Data.Repositories
{
    public class DepositAddressRepository : PostgresRepository<DepositAddress>
    {
        private readonly DataContext _context;

        public DepositAddressRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string> GetAddress(Expression<Func<DepositAddress, bool>> predicate)
        {
            var address = await _context.DepositAddresses
                .FirstOrDefaultAsync(predicate);

            if (address == null) return null;

            return address.Address;
        }
    }
}
