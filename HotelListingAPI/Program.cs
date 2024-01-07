using System.Text;
using System.Text.Json;
using HotelListingAPI.Core.Configurations;
using HotelListingAPI.Core.Middleware;
using HotelListingAPI.Core.Models.Contracts;
using HotelListingAPI.Documents;
using HotelListingAPI.Entitys;
using HotelListingAPI.Repositorys;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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

//TODO: Add healthcheck 1
builder
    .Services
    .AddHealthChecks()
    .AddCheck<CustomHealthCheck>(
        "Custom Health Check",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "custom" }
    )
    .AddNpgSql(connectionString, tags: new[] { "database" })
    .AddDbContextCheck<HotelListingDbContext>(tags: new[] { "database" });

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

// TODO: Add healthcheck2
app.MapHealthChecks(
    "/healthcheck",
    new HealthCheckOptions
    {
        Predicate = healthcheck => healthcheck.Tags.Contains("custom"),
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            [HealthStatus.Degraded] = StatusCodes.Status200OK
        },
        ResponseWriter = WriteResponse
    }
);

app.MapHealthChecks(
    "/databasehealthcheck",
    new HealthCheckOptions
    {
        Predicate = healthcheck => healthcheck.Tags.Contains("database"),
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            [HealthStatus.Degraded] = StatusCodes.Status200OK
        },
        ResponseWriter = WriteResponse
    }
);

app.MapHealthChecks(
    "/healthz",
    new HealthCheckOptions
    {
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            [HealthStatus.Degraded] = StatusCodes.Status200OK
        },
        ResponseWriter = WriteResponse
    }
);

app.MapHealthChecks("/health");

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=UTF-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status", healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description", healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(
                    jsonWriter,
                    item.Value,
                    item.Value?.GetType() ?? typeof(object)
                );
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }
        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
}

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

class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var isHealthy = true;

        /* custom check. Logic....etc.etc. */

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("All systems arte looking good"));
        }

        return Task.FromResult(
            new HealthCheckResult(context.Registration.FailureStatus, "System Unhealthy")
        );
    }
}
