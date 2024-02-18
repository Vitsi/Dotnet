/**
    Retrieves a user based on the provided username.
    Verifies the user's password.
    If the user is found and the password is valid, generates a JWT token.
    Returns the JWT token in the response.
**/
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.Ms.Models;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Ms.Controllers
{
    [ApiController]
    [Route("Login")]

    public class LoginController : ControllerBase
    {
        private readonly IService<UserModel> _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginController> _logger;

        //constructor
        public LoginController(IService<UserModel> userService, IConfiguration configuration, ILogger<LoginController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }
        private bool IsAdmin(string username)
        {
            return username.StartsWith("admin_", StringComparison.OrdinalIgnoreCase);
        }
        [AllowAnonymous]
        [HttpPost("AdminLogin")]
        public async Task<ActionResult<UserDto>> AdminLoginAsync(LoginDto loginDto)
        {
            try
            {
                var userFound = await _userService.GetAsync(user => user.Username == loginDto.Username);

                if (userFound != null)
                {
                    var isAdmin = IsAdmin(userFound.Username);
                    if (!isAdmin)
                    {
                        return Forbid(); // Return Forbidden for non-admin users
                    }
                    var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, userFound.Password);

                    if (!isPasswordValid)
                    {
                        return BadRequest("Invalid Password");
                    }
                    //Generating JWT token
                    //var token = GenerateJwtToken(userFound);
                    var token = GenerateJwtToken(userFound.Id, userFound);
                    return Ok(new UserDto(
                    userFound.Id,
                    userFound.Username,
                    userFound.EmailAddress,
                    userFound.Role,
                    userFound.CreatedDate,
                    userFound.Photo
                )
                    { Token = token });


                }
                else
                {
                    return NotFound("User not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing login.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UserDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var userFound = await _userService.GetAsync(user => user.Username == loginDto.Username);
                if (userFound == null)
                {
                    return NotFound("User not found");
                }

                //Verify pass
                var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, userFound.Password);

                if (!isPasswordValid)
                {
                    return BadRequest("Invalid Password");
                }
                //Generating JWT token
                var token = GenerateJwtToken(userFound.Id, userFound);

                // Return userdto with token
#pragma warning disable CS8604 // Possible null reference argument.
                return Ok(new UserDto(
                    userFound.Id,
                    userFound.Username,
                    userFound.EmailAddress,
                    userFound.Role,
                    userFound.CreatedDate,
                    userFound.Photo
                )
                { Token = token });
#pragma warning restore CS8604 // Possible null reference argument.


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing login.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        private string GenerateJwtToken(Guid userId, UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var isAdmin = IsAdmin(user.Username);



            var adminClaim = new Claim("IsAdmin", isAdmin.ToString());
            var claims = new[]
            {
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Name, user.Username),
               new Claim(ClaimTypes.Email, user.EmailAddress),
               new Claim(ClaimTypes.Role, user.Role),
               adminClaim
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
