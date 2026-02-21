using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.Roles;

public sealed class RoleDto : EntityDto
{
    public string Name { get; set; } = default!;
    public int PermissionsCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}
