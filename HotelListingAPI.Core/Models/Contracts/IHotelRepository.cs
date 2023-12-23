using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelListingAPI.Entitys;

namespace HotelListingAPI.Models.Contracts
{
    public interface IHotelRepository : IGenericRepository<Hotel> { }
}
