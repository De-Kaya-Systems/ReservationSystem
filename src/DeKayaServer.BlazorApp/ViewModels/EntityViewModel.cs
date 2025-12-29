namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class EntityViewModel
{
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string CreatedFullName { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string? UpdatedFullName { get; set; }
}
