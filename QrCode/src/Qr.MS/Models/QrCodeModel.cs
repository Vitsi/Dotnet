using Common;
using System;

namespace Qr.MS.Models
{
    public class QRCodeModel : IModel
    {
        public Guid Id { get; set; }
        public string? Data { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public Guid UserId { get; set; }
        public string? Username { get; set; }

         // Add reference to UserModel
        //public UserModel User { get; set; }

    }

}
