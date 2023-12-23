using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Dtos.Users
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(
            15,
            ErrorMessage = "Your Password is limited to {2} t0 {1} characters.",
            MinimumLength = 5
        )]
        public string Password { get; set; }
    }
}
