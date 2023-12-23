using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Core.Dtos.Users
{
    public class ApiUserDto : LoginDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}
