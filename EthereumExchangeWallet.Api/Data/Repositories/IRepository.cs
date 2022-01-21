using System.Collections.Generic;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetItemAsync(int id);
        Task<IEnumerable<T>> GetListAsync();
        Task<T> CreateAsync(T asset);
        Task UpdateAsync(T asset);
        Task DeleteAsync(T asset);
    }
}