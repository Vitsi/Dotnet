using Qr.MS.Models;

namespace Qr.MS
{
    public static class QRCodeExtensions
    {
        public static QRCodeDto AsDto(this QRCodeModel qrCode, Guid UserId)
        {
            return new QRCodeDto(
                qrCode.Id, 
                qrCode.Data,
                qrCode.CreatedDate,
                UserId
                );      
        }
       
    }
}
