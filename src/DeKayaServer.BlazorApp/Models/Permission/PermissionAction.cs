namespace DeKayaServer.BlazorApp.Models.Permission;

public sealed class PermissionAction
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public bool IsSelected { get; set; }
}