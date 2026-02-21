namespace DeKayaServer.Contracts.Users;

public sealed record UpdateUserRequest( 
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    Guid RoleId );