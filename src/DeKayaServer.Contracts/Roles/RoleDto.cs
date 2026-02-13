namespace DeKayaServer.Contracts.Roles;

public sealed class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedFullName { get; set; } = default!;

    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedFullName { get; set; }
    public int PermissionsCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}
