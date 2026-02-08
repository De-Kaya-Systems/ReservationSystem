namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class RoleViewModel
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string? UpdatedFullName { get; set; }
    public string? CreatedFullName { get; set; }
}