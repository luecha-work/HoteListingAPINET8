using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Dto.Country
{
    public abstract class BaseCountryDto
    {
        //TODO: Validate properties in dto with -> Required
        public virtual string Name { get; set; }
        public string ShortName { get; set; }
    }
}
