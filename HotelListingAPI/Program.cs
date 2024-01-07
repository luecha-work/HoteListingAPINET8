using System.Text;
using HotelListingAPI.Core.Configurations;
using HotelListingAPI.Core.Middleware;
using HotelListingAPI.Core.Models.Contracts;
using HotelListingAPI.Documents;
using HotelListingAPI.Entitys;
using HotelListingAPI.Repositorys;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//TODO: Connect SQL PosgregSQL Database
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder
    .Services
    .AddDbContext<HotelListingDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

// TODO: Add IdentityCore for Authen
builder
    .Services
    .AddIdentityCore<ApiUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
    })
    // .AddRoles<IdentityRole>()
    .AddRoles<Roles>()
    .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
    .AddEntityFrameworkStores<HotelListingDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerGen();

//TODO: Add cors url1
builder
    .Services
    .AddCors(options =>
    {
        options.AddPolicy("AllowAll", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
    });

//TODO: Add Mvc.Versioning from headers is(X-Version) or query(api-version)
builder
    .Services
    .AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Version"),
            new MediaTypeApiVersionReader("ver")
        );
    });

//TODO: Add Mvc ApiExplorer
builder
    .Services
    .AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

//TODO: Add Serilog1
builder
    .Host
    .UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

//TODO: Add Maper Data 1
builder.Services.AddAutoMapper(typeof(MapperConfig));

//TODO: Add Register Repositorys to Programe
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

//TODO: Add Authen JWT
builder.Services.AddJwtConfiguration(builder.Configuration);

// builder
//     .Services
//     .AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
//         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ClockSkew = TimeSpan.Zero,
//             ValidIssuer = builder.Configuration["JwtSettings:Issuer"], //TODO: Get JwtSettings Properties from applications.json
//             ValidAudience = builder.Configuration["JwtSettings:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])
//             )
//         };
//     });

builder
    .Services
    .Configure<SecurityStampValidatorOptions>(options =>
    {
        options.ValidationInterval = TimeSpan.FromDays(1); // ตั้งค่าระยะเวลาในการตรวจสอบ Refresh Token ใหม่เป็น 1 วัน
    });

//TODO: Add Caching1
builder
    .Services
    .AddResponseCaching(options =>
    {
        options.MaximumBodySize = 1024;
        options.UseCaseSensitivePaths = true;
    });

builder.Services.AddHealthChecks();

//TODO: Add OData
builder
    .Services
    .AddControllers()
    .AddOData(options =>
    {
        options.Select().Filter().OrderBy();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { }

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/healthz");

//TODO: Add Serilog2
app.UseSerilogRequestLogging();

//TODO: Add Middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

//TODO: Add cors url2
app.UseCors("AllowAll");

//TODO: Add Caching2
app.UseResponseCaching();

app.Use(
    async (context, next) =>
    {
        context.Response.GetTypedHeaders().CacheControl =
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(10),
            };
        context.Response.GetTypedHeaders().Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
            new string[] { "Accept-Encoding" };

        await next();
    }
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
