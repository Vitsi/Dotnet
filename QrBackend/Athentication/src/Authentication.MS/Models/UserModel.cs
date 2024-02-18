using Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Authentication.Ms.Models
{
    public class UserModel : IModel
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string EmailAddress { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? Role { get; set; }
        public DateTimeOffset LastLogoutTime { get; set; }
        public byte[] Photo { get; set; }

    }
}