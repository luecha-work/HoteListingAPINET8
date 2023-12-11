using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Dto.Country
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class RequiredForCreateAttribute : RequiredAttribute
    {
        // This class inherits from RequiredAttribute but has a separate name to distinguish its usage
    }

    public class CreateCountryDto : BaseCountryDto
    {
        [RequiredForCreate(ErrorMessage = "Name is required for creation")]
        public override string Name { get; set; }
    }
}
