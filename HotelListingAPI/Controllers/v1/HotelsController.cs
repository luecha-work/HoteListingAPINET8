using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelListingAPI.Dtos.Hotels;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;
using HotelListingAPI.Models.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public HotelsController(IHotelRepository hotelRepository, IMapper mapper)
        {
            this._hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        [HttpGet("all-hotels")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
        {
            var hotels = await this._hotelRepository.GetAllAsync();
            var mapHotels = this._mapper.Map<List<HotelDto>>(hotels);

            return Ok(mapHotels);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetPagedHotels(
            [FromQuery] QueryParameters queryParameters
        )
        {
            var pagedHotelsResult = await this._hotelRepository.GetPagedResultAsync<HotelDto>(
                queryParameters
            );

            return Ok(pagedHotelsResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            var hotel = await this._hotelRepository.GetAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            var mapHotel = this._mapper.Map<HotelDto>(hotel);

            return Ok(mapHotel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, HotelDto hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return BadRequest();
            }

            var hotel = await _hotelRepository.GetAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            _mapper.Map(hotelDto, hotel);

            try
            {
                await this._hotelRepository.UpdateAsync(hotel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await HotelExists(id))
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

        [HttpPost]
        public async Task<ActionResult<Hotel>> PostTModel(CreateHotelDto createHotelDto)
        {
            var hotel = _mapper.Map<Hotel>(createHotelDto);

            await this._hotelRepository.AddAsync(hotel);

            return CreatedAtAction("GetHotel", new { id = hotel.Id }, hotel);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Hotel>> DeleteHotelById(int id)
        {
            var hotel = await this._hotelRepository.GetAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            await this._hotelRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> HotelExists(int id)
        {
            return await this._hotelRepository.Exists(id);
        }
    }
}
