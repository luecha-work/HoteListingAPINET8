using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListingAPI.Entitys.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            builder.HasData(
                new Roles
                {
                    RoleCode = "R01",
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                    Create_At = DateTimeOffset.Now,
                    Update_At = DateTimeOffset.Now,
                    Create_By = "Configure",
                    Update_By = ""
                },
                new Roles
                {
                    RoleCode = "R02",
                    Name = "User",
                    NormalizedName = "USER",
                    Create_At = DateTimeOffset.Now,
                    Update_At = DateTimeOffset.Now,
                    Create_By = "Configure",
                    Update_By = ""
                }
            );
        }
    }
}
