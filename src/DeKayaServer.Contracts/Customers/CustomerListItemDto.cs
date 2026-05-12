namespace DeKayaServer.Contracts.Customers;

public sealed class CustomerListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? PhoneNumber2 { get; set; }
    public string? Email { get; set; }

    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string FullAddress { get; set; } = default!;

    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedFullName { get; set; } = default!;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedFullName { get; set; }
}