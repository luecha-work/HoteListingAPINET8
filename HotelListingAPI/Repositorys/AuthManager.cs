using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HotelListingAPI.Dtos.Users;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HotelListingAPI.Repositorys
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApiUser _user;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";

        public AuthManager(
            IMapper mapper,
            UserManager<ApiUser> userManager,
            IConfiguration configuration
        )
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _user = await this._userManager.FindByEmailAsync(loginDto.Email);

            bool isValidUser = await this._userManager.CheckPasswordAsync(_user, loginDto.Password);

            if (_user == null || isValidUser == false)
            {
                return null;
            }

            var token = await this.GenerateToken();

            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = this._mapper.Map<ApiUser>(userDto);

            _user.UserName = userDto.Email;

            var result = await this._userManager.CreateAsync(_user, userDto.Password);

            if (result.Succeeded)
            {
                await this._userManager.AddToRoleAsync(_user, "User");
            }

            return result.Errors;
        }

        private async Task<string> GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(this._configuration["JwtSettings:Key"]) //TODO: Get Key from JwtSettings in  applications.json
            );

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = await this._userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await this._userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
                .Union(userClaims)
                .Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: this._configuration["JwtSettings:Issuer"],
                audience: this._configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime
                    .Now
                    .AddMinutes(
                        Convert.ToInt32(this._configuration["JwtSettings:DurationInMinutes"])
                    ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRefreshToken()
        {
            Console.WriteLine("start refresh token 1");
            await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);

            var newRefreshToken = await _userManager.GenerateUserTokenAsync(
                _user,
                _loginProvider,
                _refreshToken
            );
            Console.WriteLine("GenerateUserTokenAsynced.");
            var result = await _userManager.SetAuthenticationTokenAsync(
                _user,
                _loginProvider,
                _refreshToken,
                newRefreshToken
            );

            Console.WriteLine("to return");
            return newRefreshToken;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);

            var username = tokenContent
                .Claims
                .ToList()
                .FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)
                ?.Value;

            this._user = await this._userManager.FindByNameAsync(username);

            if (_user == null || _user.Id != request.UserId)
            {
                return null;
            }

            if (this._user == null)
            {
                return null;
            }

            var isValidRefreshToken = await this._userManager.VerifyUserTokenAsync(
                _user,
                _loginProvider,
                _refreshToken,
                request.RefreshToken
            );

            if (isValidRefreshToken)
            {
                var token = await this.GenerateToken();

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await this.CreateRefreshToken()
                };
            }

            await this._userManager.UpdateSecurityStampAsync(_user);

            return null;
        }
    }
}
