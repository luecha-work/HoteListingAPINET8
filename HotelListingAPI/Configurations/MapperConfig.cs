using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelListingAPI.Dto.Country;
using HotelListingAPI.Dto.Hotels;
using HotelListingAPI.Entitys;

//TODO: Add Maper Data 2
namespace HotelListingAPI.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //TODO: Create Mapper -> Country to Dto
            CreateMap<Country, CreateCountryDto>()
                .ReverseMap();
            CreateMap<Country, UpdateCountryDto>().ReverseMap();
            CreateMap<Country, GetCountryDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();

            //TODO: Create Mapper -> Hotel to Dto
            CreateMap<Hotel, HotelDto>()
                .ReverseMap();
        }
    }
}
