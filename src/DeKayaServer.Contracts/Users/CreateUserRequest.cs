namespace DeKayaServer.Contracts.Users;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    Guid RoleId
);
