using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelListingAPI.Configuration
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureSwaggerGen(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo { Title = "Hotel Listing API", Version = "v1" }
                );
                options.AddSecurityDefinition(
                    JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Description =
                            @"JWT Authorization header using the Bearer scheme.
                            Enter 'Bearer' [space] and then your token in the text input below.
                            Example: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme
                    }
                );

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = JwtBearerDefaults.AuthenticationScheme
                                },
                                Scheme = "oauth2",
                                Name = JwtBearerDefaults.AuthenticationScheme,
                                In = ParameterLocation.Header
                            },
                            new List<string>()
                        }
                    }
                );
            });
        }
    }
}
