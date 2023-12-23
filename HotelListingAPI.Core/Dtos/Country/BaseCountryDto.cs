using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Dtos.Country
{
    public abstract class BaseCountryDto
    {
        public virtual string Name { get; set; }
        public string ShortName { get; set; }
    }
}
