using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelListingAPI.Dtos.Users;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;
using Microsoft.AspNetCore.Identity;

namespace HotelListingAPI.Repositorys
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager)
        {
            this._mapper = mapper;
            this._userManager = userManager;
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            var user = this._mapper.Map<ApiUser>(userDto);

            user.UserName = userDto.Email;

            var result = await this._userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await this._userManager.AddToRoleAsync(user, "User");
            }

            return result.Errors;
        }
    }
}
