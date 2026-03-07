using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.Customers;

public sealed class CreateCustomerRequest
{
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public Address? Address { get; init; }
    public Contact? Contact { get; init; }
    public bool IsActive { get; init; }
};
