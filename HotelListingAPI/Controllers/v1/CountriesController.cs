using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelListingAPI.Dtos.Country;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CountriesController : ControllerBase
    {
        //TODO: setup to use auto-mapper data 1
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(
            IMapper mapper,
            ICountriesRepository countriesRepository,
            ILogger<CountriesController> logger
        )
        {
            //TODO: setup to use auto-mapper data 2
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
            this._logger = logger;
        }

        // GET: api/Countries
        [HttpGet]
        // [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();

            //TODO: use auto-mapper
            var records = this._mapper.Map<List<GetCountryDto>>(countries);

            return Ok(records);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            this._logger.LogWarning($"Record found in {nameof(GetCountry)}. with Id: {id}. ");
            var country = await _countriesRepository.GetDetails(id);

            Console.WriteLine("country" + country);

            if (country == null)
            {
                return NotFound();
            }

            var countryDto = _mapper.Map<CountryDto>(country);

            return Ok(countryDto);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Record Id");
            }

            // _context.Entry(country).State = EntityState.Modified;

            var country = await _countriesRepository.GetAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            this._mapper.Map(updateCountryDto, country);

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDto)
        {
            //TODO: use auto-mapper
            var country = _mapper.Map<Country>(createCountryDto);

            await _countriesRepository.AddAsync(country);

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize("Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            await _countriesRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
