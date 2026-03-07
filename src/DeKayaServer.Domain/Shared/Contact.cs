namespace DeKayaServer.Domain.Shared;

public sealed class Contact
{
    public string PhoneNumber { get; init; } = default!;
    public string? PhoneNumber2 { get; init; }
    public string? Email { get; init; }
}
