namespace DeKayaServer.BlazorApp.Models.Permission;

public sealed class PermissionGroup
{
    public required string GroupKey { get; init; }
    public required string Title { get; init; }
    public required List<PermissionAction> Actions { get; init; }
}
