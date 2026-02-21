using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Services;
using DeKayaServer.BlazorApp.ViewModels;

namespace DeKayaServer.BlazorApp.Components.Pages.Header.Notification;

public partial class NotificationComponent(
    ToastService toastService,
    INotificationPanelService notificationPanelService ) : IDisposable
{
    private List<NotificationPanelViewModel> Notifications { get; set; } = [];
    private bool isLoading = true;

    private Action<ToastInput>? toastShowHandler;
    private Action? notificationStateHandler;

    private int UnreadCount => Notifications.Count( n => !n.IsRead );

    protected override Task OnInitializedAsync()
    {
        toastShowHandler = toast => _ = SafeRunAsync( () => HandleToastShow( toast ) );
        toastService.Subscribe( toastShowHandler );

        notificationStateHandler = () => _ = SafeRunAsync( HandleNotificationStateChanged );
        notificationPanelService.StateChanged += notificationStateHandler;

        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync( bool firstRender )
    {
        if ( !firstRender )
            return;

        await LoadNotificationsAsync();
    }

    private async Task HandleToastShow( ToastInput toast )
    {
        await notificationPanelService.AddNotificationAsync( toast.Title, toast.Message );
        await LoadNotificationsAsync();
    }

    private async Task HandleNotificationStateChanged()
    {
        await LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        isLoading = true;
        Notifications = await notificationPanelService.GetNotificationsAsync();
        isLoading = false;

        await InvokeAsync( StateHasChanged );
    }

    private async Task RemoveAsync( Guid id )
    {
        await notificationPanelService.RemoveNotificationAsync( id );
        await LoadNotificationsAsync();
    }

    private async Task MarkAsReadAsync( Guid id )
    {
        await notificationPanelService.MarkAsReadAsync( id );
        await LoadNotificationsAsync();
    }

    private async Task MarkAllAsReadAsync()
    {
        await notificationPanelService.MarkAllAsReadAsync();
        await LoadNotificationsAsync();
    }

    private async Task ClearAllAsync()
    {
        await notificationPanelService.ClearAllAsync();
        await LoadNotificationsAsync();
    }

    private static async Task SafeRunAsync( Func<Task> action )
    {
        try
        {
            await action();
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        if ( toastShowHandler is not null )
            toastService.UnSubscribe( toastShowHandler );

        if ( notificationStateHandler is not null )
            notificationPanelService.StateChanged -= notificationStateHandler;
    }
}