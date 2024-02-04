using Authentication.Ms.Models;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Ms.Controllers
{
    [ApiController]
    [Route("Useraccount")]
    public class UseraccountController : ControllerBase
    {
        private readonly IService<UserModel> _userService;
        private readonly ILogger<UseraccountController> _logger;

        public UseraccountController(IService<UserModel> userService, ILogger<UseraccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync()
        {
            try
            {
                var users = (await _userService.GetAllAsync()).Select(user => user.AsDto());
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting users.");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("ByUsername/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userService.GetAsync(u => u.Username == username);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user.AsDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting a user by username.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserAsync(Guid id)
        {
            try
            {
                //cheack is user making request is admin
                if (User.IsInRole("Admin"))
                {
                    //var user = await _userService.GetByUsernameAsync(username);
                    var user = await _userService.GetAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }
                    await _userService.RemoveAsync(user.Id);
                    return NoContent();
                }
                else
                {
                    //user isnt admin
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a user.");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("ByUsername/{username}")]
        public async Task<ActionResult> DeleteUserByUsernameAsync(string username)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    // Use the GetAsync method to find the user by username
                    var user = await _userService.GetAsync(u => u.Username == username);
                    if (user == null)
                    {
                        return NotFound();
                    }
                    await _userService.RemoveAsync(user.Id);
                    return NoContent();
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a user.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "User")]
        [HttpPut("User/{id}")]
        public async Task<ActionResult> UpdatedUserAsync(Guid id, [FromForm] UpdateUserDto updateUserDto)
        {
            try
            {
                //get user making request
                var requestingUser = await _userService.GetAsync(id);
                //cheack if user rquesting samr as user updated or is an admin
                if (requestingUser != null && requestingUser.Id == id)
                {
                    var existingUser = await _userService.GetAsync(id);

                    //cheack if user exists
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    //optional editing
                    if (!string.IsNullOrEmpty(updateUserDto.Username))
                    {
                        existingUser.Username = updateUserDto.Username;
                    }
                    if (!string.IsNullOrEmpty(updateUserDto.EmailAddress))
                    {
                        existingUser.EmailAddress = updateUserDto.EmailAddress;
                    }
                    if (!string.IsNullOrEmpty(updateUserDto.Password))
                    {
                        existingUser.Password = EncryptPassword(updateUserDto.Password);
                    }
                    // handle profile picture
                    if (updateUserDto.Photo != null)
                    {
                        existingUser.Photo = updateUserDto.Photo;
                    }

                    await _userService.UpdateAsync(existingUser);
                    return NoContent();
                }
                else
                {
                    //not permited to edit
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a user.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatedAdminAsync(Guid id, [FromForm] UpdateUserDto updateUserDto)
        {
            try
            {
                //get user making request
                var requestingUser = await _userService.GetAsync(id);
                //cheack if user rquesting samr as user updated or is an admin
                if (requestingUser != null && requestingUser.Id == id)
                {
                    var existingUser = await _userService.GetAsync(id);

                    //cheack if user exists
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    //optional editing
                    if (!string.IsNullOrEmpty(updateUserDto.Username))
                    {
                        existingUser.Username = updateUserDto.Username;
                    }
                    if (!string.IsNullOrEmpty(updateUserDto.EmailAddress))
                    {
                        existingUser.EmailAddress = updateUserDto.EmailAddress;
                    }
                    if (!string.IsNullOrEmpty(updateUserDto.Password))
                    {
                        existingUser.Password = EncryptPassword(updateUserDto.Password);
                    }
                    // handle profile picture
                    if (updateUserDto.Photo != null)
                    {
                        existingUser.Photo = updateUserDto.Photo;
                    }

                    await _userService.UpdateAsync(existingUser);
                    return NoContent();
                }
                else
                {
                    //not permited to edit
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a user.");
                return StatusCode(500, "Internal Server Error");
            }
        }


        private static string EncryptPassword(string password)
        {
            // Generate a random salt
            string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            // Hash the password with the generated salt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }
    }
}