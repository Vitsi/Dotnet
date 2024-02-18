namespace Qr.MS
{
    public record QRCodeDto(Guid Id, string Data, DateTimeOffset CreatedDate, Guid UserId);
    
    public record SaveQRCodeRequestDto(string Data,DateTimeOffset CreatedDate);
}
