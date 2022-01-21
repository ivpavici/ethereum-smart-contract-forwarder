using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Data.Repositories
{
    public class PostgresRepository<T> : IRepository<T> where T : class
    {
        private readonly DataContext _context;
        internal DbSet<T> dbSet;

        public PostgresRepository(DataContext context)
        {
            _context = context;
            dbSet = context.Set<T>();
        }

        public async Task<T> CreateAsync(T entity)
        {
            var result = await dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<T> GetItemAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetListAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
