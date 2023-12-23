using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;
using HotelListingAPI.Models.Query;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Repositorys
{
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        //TODO: Setup DbContext to use on Repositorys
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<T> AddAsync(T entity)
        {
            await this._context.AddAsync(entity);
            await this._context.SaveChangesAsync();

            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);
            this._context.Set<T>().Remove(entity);
            await this._context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);

            return entity != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await this._context.Set<T>().ToListAsync();
        }

        public async Task<T> GetAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }

            return await this._context.Set<T>().FindAsync(id);
        }

        public async Task<PagedResult<TResult>> GetPagedResultAsync<TResult>(
            QueryParameters queryParameters
        )
        {
            var totalSize = await this._context.Set<T>().CountAsync();

            var item = await this._context
                .Set<T>()
                .Skip(queryParameters.StartIndex)
                .Take(queryParameters.PageSize)
                .ProjectTo<TResult>(this._mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResult<TResult>
            {
                Item = item,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize
            };
        }

        public async Task UpdateAsync(T entity)
        {
            this._context.Update(entity);
            await this._context.SaveChangesAsync();
        }
    }
}
