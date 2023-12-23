using HotelListingAPI.Core.Dtos.Hotels;

namespace HotelListingAPI.Core.Dtos.Country
{
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public List<HotelDto> Hotels { get; set; }
    }
}
