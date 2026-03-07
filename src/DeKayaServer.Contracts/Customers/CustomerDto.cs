using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.Customers;

public sealed class CustomerDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;

    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string FullAddress { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;
    public string? PhoneNumber2 { get; set; }
    public string? Email { get; set; }
}
