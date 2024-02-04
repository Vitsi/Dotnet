using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;



namespace Authentication.Ms.Controllers
{
    [ApiController]
    [Route("Logout")]
    [Authorize] // Only allow authenticated users to log out
    public class LogoutController : ControllerBase
    {
        private readonly ILogger<LogoutController> _logger;

        public LogoutController(ILogger<LogoutController> logger)
        {
            _logger = logger;
        }
        
        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            try
            {
                // Get user's username from claims
                var usernameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (usernameClaim != null)
                {
                    var username = usernameClaim.Value;

                    // Sign out the current user using the authentication scheme (e.g., Cookies)
                    await HttpContext.SignOutAsync("Bearer");

                    return Ok("Logout successful");
                }

                return BadRequest("Unable to determine user");
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while processing logout.");
                return StatusCode(500, "Internal Server Error");
            }
        }          
    }
}
