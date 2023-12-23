using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using HotelListingAPI.Dtos.Users;
using HotelListingAPI.Entitys;
using HotelListingAPI.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HotelListingAPI.Repositorys
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private ApiUser _user;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";

        public AuthManager(
            IMapper mapper,
            UserManager<ApiUser> userManager,
            IConfiguration configuration,
            ILogger<AuthManager> logger
        )
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _logger.LogWarning($"Looking for user with email {loginDto.Email}");

            _user = await this._userManager.FindByEmailAsync(loginDto.Email);

            bool isValidUser = await this._userManager.CheckPasswordAsync(_user, loginDto.Password);

            if (_user == null || isValidUser == false)
            {
                _logger.LogWarning($"User with email {loginDto.Email} was not found.");

                return null;
            }

            var token = await this.GenerateToken();
            _logger.LogWarning(
                $"Token Generated for user with email {loginDto.Email} | Tokren {token}"
            );

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
            this._logger.LogWarning($"CreateRefreshToken -> {_user != null}");
            if (_user != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(
                    _user,
                    _loginProvider,
                    _refreshToken
                );

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
                Console.WriteLine($"newRefreshToken is {newRefreshToken}");
                return newRefreshToken;
            }
            else
            {
                // Handle the case where _user is null, e.g., log an error or throw an exception
                Console.WriteLine("_user is null. Cannot create refresh token.");
                // You might want to throw an exception or return an error message here.
                return null; // or throw new SomeException("User is null");
            }
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
