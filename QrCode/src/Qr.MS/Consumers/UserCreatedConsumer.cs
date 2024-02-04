using Auth.Contracts;
using Common;
using MassTransit;
using Qr.MS.Models;

namespace Qr.MS.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly IService<UserModel> _userService;

        public UserCreatedConsumer(IService<UserModel> userService)
        {
            _userService = userService;
        }
        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            var message = context.Message;
            var user = await _userService.GetAsync(message.Id);
            if ( user != null)
            {
                return;
            }

            user = new UserModel
            {
                Id = message.Id,
                Username = message.Username
            };

            await _userService.CreateAsync(user);
        }
        
    }
}