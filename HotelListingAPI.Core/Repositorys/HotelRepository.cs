using AutoMapper;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;

namespace HotelListingAPI.Repositorys
{
    public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
    {
        public HotelRepository(HotelListingDbContext context, IMapper mapper)
            : base(context, mapper) { }
    }
}
