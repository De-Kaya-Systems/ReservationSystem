namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class NotificationPanelViewModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsRead { get; set; } = false;
}
