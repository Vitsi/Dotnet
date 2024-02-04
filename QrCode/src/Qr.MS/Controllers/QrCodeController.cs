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
using Auth.Contracts;
using MassTransit;


namespace Qr.MS.Controllers
{
    [ApiController]
    [Route("QRCode")]

    public class QRCodeController : ControllerBase
    {
        private readonly IService<QRCodeModel> _qrCodeService;

        private readonly IService<UserModel> _userService;

        private readonly IRequestClient<UserCreated> _userCreatedRequestClient;

        private readonly ILogger<QRCodeController> _logger;


        public QRCodeController(IService<QRCodeModel> qrCodeService,
        IService<UserModel> userService,
        IRequestClient<UserCreated> userCreatedRequestClient,
        ILogger<QRCodeController> logger)
        {
            _qrCodeService = qrCodeService;
            _userService = userService;
            _userCreatedRequestClient = userCreatedRequestClient;
            _logger = logger;

        }

        [HttpGet("{id}", Name = "GetQrCodeById")]
        public async Task<ActionResult<QRCodeDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var qrcode = await _qrCodeService.GetAsync(q => q.Id == id);
                //var user = await _userService.GetAsync(u => u.Id == id);
                if (qrcode == null)
                {
                    return NotFound("Qr code not found");
                };

                var userId = qrcode.UserId;
                // Retrieve user information based on UserId
                var user = await _userService.GetAsync(u => u.Id == userId);
               // var user = await _userService.GetAllAsync();
                if (user == null)
                {
                    // Handle the case when the user associated with the QR code is not found
                    return NotFound("User not found for the provided QR code");
                }

                //  return Ok(qrcode.AsDto());
                return Ok(new QRCodeDto(qrcode.Id, qrcode.Data, qrcode.CreatedDate, user.Id));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting qrcode by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<SaveQRCodeRequestDto>> SaveQrCodeAsync([FromBody] SaveQRCodeRequestDto request)
        {
            try
            {

                if (User == null)
                {
                    return NotFound("User Id not found in token");
                }
             

                var newQrCode = new QRCodeModel
                {
                    Id = Guid.NewGuid(),
                    Data = request.Data,
                    CreatedDate = DateTimeOffset.UtcNow,
                    UserId = request.UserId
                };

               

                await _qrCodeService.CreateAsync(newQrCode);
                var qrCodeDto = newQrCode;
                return CreatedAtRoute("GetQrCodeById", new { id = newQrCode.Id }, qrCodeDto);

            }
            catch (Exception)
            {
                _logger.LogError("An error occured while trying to save qrcode");
                return StatusCode(500, "Internal server error. Check the logs for details.");
            }
        }

    }

}


