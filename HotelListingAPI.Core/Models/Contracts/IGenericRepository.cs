using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelListingAPI.Models.Query;

namespace HotelListingAPI.Models.Contracts
{
    public interface IGenericRepository<T>
        where T : class
    {
        Task<T> GetAsync(int? id);
        Task<List<T>> GetAllAsync();
        Task<PagedResult<TResult>> GetPagedResultAsync<TResult>(QueryParameters queryParameters);
        Task<T> AddAsync(T entity);
        Task DeleteAsync(int id);
        Task UpdateAsync(T entity);
        Task<bool> Exists(int id);
    }
}
