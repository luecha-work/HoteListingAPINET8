using AutoMapper;
using HotelListingAPI.Core.Dtos.Country;
using HotelListingAPI.Core.Dtos.Hotels;
using HotelListingAPI.Core.Dtos.Users;
using HotelListingAPI.Entitys;

//TODO: Add Maper Data 2
namespace HotelListingAPI.Core.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //TODO: Create Mapper -> Country to Dtos
            CreateMap<Country, CreateCountryDto>()
                .ReverseMap();
            CreateMap<Country, UpdateCountryDto>().ReverseMap();
            CreateMap<Country, GetCountryDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();

            //TODO: Create Mapper -> Hotel to Dtos
            CreateMap<Hotel, HotelDto>()
                .ReverseMap();
            CreateMap<Hotel, CreateHotelDto>().ReverseMap();

            CreateMap<ApiUserDto, ApiUser>().ReverseMap();
        }
    }
}
