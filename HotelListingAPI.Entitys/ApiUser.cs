
using Microsoft.AspNetCore.Identity;

namespace HotelListingAPI.Entitys
{
    public class ApiUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
