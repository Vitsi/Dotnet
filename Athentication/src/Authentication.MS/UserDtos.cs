using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Authentication.Ms
{
    public record UserDto(Guid Id, string Username, string EmailAddress, string Role, DateTimeOffset CreatedDate, byte[] Photo)
    {
        public string Token { get; internal set; }
    }

    public record CreateUserDto(string Username, string EmailAddress, string Password);

    public record UpdateUserDto(string Username, string EmailAddress, string Password, byte[] Photo);

    public record LoginDto(string Username, string Password);
}
