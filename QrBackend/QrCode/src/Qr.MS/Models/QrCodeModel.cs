using Common;
using System;

namespace Qr.MS.Models
{
    public class QRCodeModel : IModel
    {
        public Guid Id { get; set; }
        public string? Data { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        
        //id from authentication
        public Guid UserId { get; set; }

    }

}
