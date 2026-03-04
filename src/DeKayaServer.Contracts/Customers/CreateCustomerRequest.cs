namespace DeKayaServer.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string City,
    string District,
    string FullAddress,
    string PhoneNumber,
    string PhoneNumber2,
    string Email );
