using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.Users;

public sealed class UserDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    public Guid RoleId { get; set; }
}
