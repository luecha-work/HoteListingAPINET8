using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelListingAPI.Dtos.Country;
using HotelListingAPI.Dtos.Hotels;
using HotelListingAPI.Dtos.Users;
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
