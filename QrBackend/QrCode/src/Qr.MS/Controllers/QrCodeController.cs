using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Logging;
using Qr.MS.Models;
using Microsoft.AspNetCore.Authorization;
using DnsClient.Protocol;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Qr.MS;


namespace Qr.MS.Controllers
{
    [ApiController]
    [Route("QRCode")]

    public class QRCodeController : ControllerBase
    {
        private readonly IService<QRCodeModel> _qrCodeService;
        private readonly ILogger<QRCodeController> _logger;


        public QRCodeController(IService<QRCodeModel> qrCodeService, ILogger<QRCodeController> logger)
        {
            _qrCodeService = qrCodeService;
            _logger = logger;

        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<QRCodeDto>>> GetAllAsync()
        {
            try
            {
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Fetch all QR codes, or modify this according to your data access layer
                var allQRCodes = await _qrCodeService.GetAllAsync();

                // Filter QR codes based on user ID if needed
                var userQRCodes = allQRCodes
                    .Where(q => q.UserId.ToString() == userIdFromToken)
                    .Select(q => q.AsDto());

                return Ok(userQRCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all QR codes");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("{id}", Name = "GetQrCodeById")]
        [Authorize]
        public async Task<ActionResult<QRCodeDto>> GetByIdAsync(Guid id)
        {
            try
            {
                // Get the user ID from the JWT token
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var qrcode = await _qrCodeService.GetAsync(q => q.Id == id);
                if (qrcode == null)
                {
                    return NotFound("Qr code not found");
                };
                // //? what is a non delegate type object, is it what happens when invoke a method wihtout ()
                // return Ok(qrcode.AsDto());
                // Check if the user making the request is the creator of the QR code
                if (qrcode.UserId.ToString() != userIdFromToken)
                {
                    // If not the creator, return unauthorized or handle it as needed
                    return Unauthorized("You are not authorized to access this QR code");
                }

                return Ok(qrcode.AsDto());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting qrcode by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SaveQRCodeRequestDto>> SaveQrCodeAsync([FromBody] SaveQRCodeRequestDto request)
        {
            try
            {
                // Get the user info from JWT
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var userClaims = identity.Claims;
                if (User == null)
                {
                    return NotFound("User Id not found in token");
                }

                // Check if the QR code with the same data already exists for the user
                var existingQRCode = await _qrCodeService.GetAsync(qr => qr.Data == request.Data);

                if (existingQRCode != null)
                {
                    // If the QR code already exists for the user, return a conflict response
                    return Conflict("QR code already saved");
                }
                var newQrCode = new QRCodeModel
                {
                    Id = Guid.NewGuid(),
                    Data = request.Data,
                    CreatedDate = DateTimeOffset.UtcNow,
                    UserId = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier) != null ? Guid.Parse(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value) : Guid.Empty,

                };
                await _qrCodeService.CreateAsync(newQrCode);
                var qrCodeDto = newQrCode.AsDto();
                return CreatedAtRoute("GetQrCodeById", new { id = newQrCode.Id }, qrCodeDto);

            }
            catch (Exception)
            {
                _logger.LogError("An error occured while trying to save qrcode");
                return StatusCode(500, "Internal server error. Check the logs for details.");
            }
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserAsync(Guid id)
        {
            try
            {

                var qrCode = await _qrCodeService.GetAsync(id);
                if (qrCode == null)
                {
                    return NotFound();
                }
                await _qrCodeService.RemoveAsync(qrCode.Id);
                return NoContent();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a user.");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }

}


