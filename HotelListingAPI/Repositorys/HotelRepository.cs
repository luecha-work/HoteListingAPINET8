using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;

namespace HotelListingAPI.Repositorys
{
    public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
    {
        public HotelRepository(HotelListingDbContext context)
            : base(context) { }
    }
}
