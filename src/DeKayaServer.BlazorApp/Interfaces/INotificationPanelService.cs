using DeKayaServer.BlazorApp.ViewModels;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface INotificationPanelService
{
    bool HasNotification { get; set; }

    event Action? StateChanged;

    Task AddNotificationAsync( string title, string message );
    Task<List<NotificationPanelViewModel>> GetNotificationsAsync();
    Task RemoveNotificationAsync( Guid id );
    Task SaveNotificationsAsync( List<NotificationPanelViewModel> notifications );

    Task MarkAsReadAsync( Guid id );
    Task MarkAllAsReadAsync();
    Task ClearAllAsync();
}