namespace DeKayaServer.Contracts.Roles;

public sealed record UpdateRoleRequest( Guid Id, string Name, bool IsActive );
