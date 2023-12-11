using System.Reflection.Emit;
using HotelListingAPI.Configurations;
using HotelListingAPI.Contracts;
using HotelListingAPI.Entitys;
using HotelListingAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//TODO: Connect SQL Server Database
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder
    .Services
    .AddDbContext<HotelListingDbContext>(options =>
    {
        options.UseSqlServer(connectionString);
    });

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//TODO: Add cors url1
builder
    .Services
    .AddCors(options =>
    {
        options.AddPolicy("AllowAll", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
    });

//TODO: Add Serilog1
builder
    .Host
    .UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

//TODO: Add Maper Data 1
builder.Services.AddAutoMapper(typeof(MapperConfig));

//TODO: Add Repository modedl
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//TODO: Add Serilog2
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

//TODO: Add cors url2
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
