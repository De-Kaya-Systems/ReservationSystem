namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class BreadcrumbItemViewModel
{
    public required string Title { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
}
