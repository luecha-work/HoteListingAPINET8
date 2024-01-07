using AutoMapper;
using HotelListingAPI.Core.Models.Contracts;
using HotelListingAPI.Entitys;

namespace HotelListingAPI.Repositorys
{
    public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
    {
        public HotelRepository(HotelListingDbContext context, IMapper mapper)
            : base(context, mapper) { }
    }
}
