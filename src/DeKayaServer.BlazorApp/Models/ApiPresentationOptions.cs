namespace DeKayaServer.BlazorApp.Models;

public sealed class ApiPresentationOptions
{
    public bool ShowError { get; init; } = true;
    public bool ShowSuccess { get; init; } = false;
    public string? SuccessMessage { get; init; }
    public string? ErrorTitle { get; init; }
    public bool CollapseMultipleErrors { get; init; } = true;
}
