namespace DeKayaServer.BlazorApp.Components.Layout.NavMenu.Model;

public sealed class NavigationItem
{
    public required string Title { get; init; }
    public string? Url { get; init; }
    public string? Icon { get; init; }
    public List<NavigationItem> Children { get; init; } = [];
}
