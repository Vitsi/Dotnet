namespace Auth.Contracts

{
    public record UserCreated(Guid Id, string Username);

    public record UserUpdate(Guid Id, string Username);

    public record UserDelete(Guid Id);
}