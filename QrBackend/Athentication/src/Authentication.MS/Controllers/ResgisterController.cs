using System;
using Authentication.Ms.Models;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Ms.Controllers
{
    [ApiController]
    [Route("Register")]
    public class RegisterController : ControllerBase
    {
        private readonly IService<UserModel> _userService;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IService<UserModel> userService, ILogger<RegisterController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UserDto>> RegisterAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Check if the username is already taken
                var existingUser = await _userService.GetAsync(user => user.Username == createUserDto.Username);
                if (existingUser != null)
                {
                    return BadRequest("Username is already taken");
                }

                // Check if the email address is already registered
                existingUser = await _userService.GetAsync(user => user.EmailAddress == createUserDto.EmailAddress);
                if (existingUser != null)
                {
                    return BadRequest("Email address is already registered");
                }

                // Hashing password before storing in db
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

                // Determine the role based on some criteria (e.g., username or email)
                string role = IsAdmin(createUserDto.Username) ? "Admin" : "User";

                // Creating a new user with the determined role
                var newUser = new UserModel
                {
                    Username = createUserDto.Username,
                    EmailAddress = createUserDto.EmailAddress,
                    Password = hashedPassword,
                    Role = role,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                // Saving user in the db
                await _userService.CreateAsync(newUser);

                return Ok("Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing registration.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        private bool IsAdmin(string username)
        {
            // cheack if the username starts with "admin_"
            return username.StartsWith("admin_", StringComparison.OrdinalIgnoreCase);
        }
    }
}
