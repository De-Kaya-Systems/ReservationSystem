using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.ViewModels;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DeKayaServer.BlazorApp.Services;

public class NotificationPanelService(
    ProtectedSessionStorage protectedSessionStorage ) : INotificationPanelService
{

    public event Action? StateChanged;
    public bool HasNotification { get; set; }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public async Task<List<NotificationPanelViewModel>> GetNotificationsAsync()
    {
        var result = await protectedSessionStorage.GetAsync<List<NotificationPanelViewModel>>( StorageKeyConstants.NotificationsPanel );

        var notifications = result.Success && result.Value is not null
            ? result.Value
            : [];

        var hasNotification = notifications.Any( n => !n.IsRead );
        if ( HasNotification != hasNotification )
        {
            HasNotification = hasNotification;
        }
        return notifications.OrderByDescending( x => x.CreatedAt ).ToList();
    }
    public async Task SaveNotificationsAsync( List<NotificationPanelViewModel> notifications )
    {
        if ( notifications.Any() )
        {
            await protectedSessionStorage.SetAsync( StorageKeyConstants.NotificationsPanel, notifications );
        }
        else
        {
            await protectedSessionStorage.DeleteAsync( StorageKeyConstants.NotificationsPanel );
        }
        HasNotification = notifications.Any( n => !n.IsRead );
        NotifyStateChanged();
    }

    public async Task AddNotificationAsync( string title, string message )
    {
        var notifications = await GetNotificationsAsync();

        notifications.Add( new NotificationPanelViewModel
        {
            Title = title,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow
        } );
        await SaveNotificationsAsync( notifications );
    }

    public async Task RemoveNotificationAsync( Guid id )
    {
        var notifications = await GetNotificationsAsync();
        var notificationToRemove = notifications.FirstOrDefault( n => n.Id == id );

        if ( notificationToRemove is not null )
        {
            notifications.Remove( notificationToRemove );
            await SaveNotificationsAsync( notifications );
        }
    }

    public async Task MarkAsReadAsync( Guid id )
    {
        var notifications = await GetNotificationsAsync();
        var item = notifications.FirstOrDefault( n => n.Id == id );

        if ( item is not null && !item.IsRead )
        {
            item.IsRead = true;
            await SaveNotificationsAsync( notifications );
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        var notifications = await GetNotificationsAsync();
        var changed = false;

        foreach ( var item in notifications.Where( x => !x.IsRead ) )
        {
            item.IsRead = true;
            changed = true;
        }

        if ( changed )
        {
            await SaveNotificationsAsync( notifications );
        }
    }

    public async Task ClearAllAsync()
    {
        await SaveNotificationsAsync( [] );
    }
}
