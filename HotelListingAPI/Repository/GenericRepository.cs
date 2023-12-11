using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelListingAPI.Contracts;
using HotelListingAPI.Entitys;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Repository
{
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        //TODO: Setup DbContext to use on Repository
        private readonly HotelListingDbContext _context;

        public GenericRepository(HotelListingDbContext context)
        {
            this._context = context;
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

        public async Task UpdateAsync(T entity)
        {
            this._context.Update(entity);
            await this._context.SaveChangesAsync();
        }
    }
}
