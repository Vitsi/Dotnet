using Common;
using System;

namespace Qr.MS.Models
{
    public class UserModel : IModel
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }

    }

}
