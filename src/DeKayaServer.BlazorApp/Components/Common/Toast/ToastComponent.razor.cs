using DeKayaServer.BlazorApp.Services;

namespace DeKayaServer.BlazorApp.Components.Common.Toast;

public partial class ToastComponent(ToastService toastService) : IDisposable
{
    private List<ToastDisplayModel> activeToasts = new();
    private readonly Dictionary<string, Timer> toastTimers = new();

    protected override void OnInitialized()
    {
        toastService.Subscribe(ShowToast, HideToastById);
    }

    private async void ShowToast(ToastInput input)
    {
        var displayModel = new ToastDisplayModel
        {
            Id = input.Id,
            Title = input.Title,
            Message = input.Message,
            ToastType = input.ToastType,
            IsVisible = true
        };
        activeToasts.Add(displayModel);
        await InvokeAsync(StateHasChanged);

        await Task.Delay(100);
        displayModel.IsVisible = true;
        await InvokeAsync(StateHasChanged);

        if (input.Duration > 0)
        {
            var timer = new Timer(async _ => await HideToastAsync(displayModel), null, input.Duration, Timeout.Infinite);
            toastTimers[displayModel.Id] = timer;
        }
    }
    private async void HideToastById(string toastId)
    {
        var toast = activeToasts.FirstOrDefault(t => t.Id == toastId);
        if (toast != null)
        {
            await HideToastAsync(toast);
        }
    }

    private async void HideToast(ToastDisplayModel toast)
    {
        await HideToastAsync(toast);
    }

    private async Task HideToastAsync(ToastDisplayModel toast)
    {
        if (toastTimers.TryGetValue(toast.Id, out var timer))
        {
            timer.Dispose();
            toastTimers.Remove(toast.Id);
        }

        toast.IsVisible = false;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(300);
        activeToasts.Remove(toast);
        await InvokeAsync(StateHasChanged);
    }

    private string GetToastClasses(ToastTypeEnum toastType) => toastType switch
    {
        ToastTypeEnum.Success => "toast-success",
        ToastTypeEnum.Error => "toast-error",
        ToastTypeEnum.Warning => "toast-warning",
        ToastTypeEnum.Info => "toast-info",
        _ => "toast-secondary",
    };

    private string GetCloseButtonClasses(ToastTypeEnum toastType) => toastType switch
    {
        ToastTypeEnum.Success => "btn-success",
        ToastTypeEnum.Error => "btn-error",
        ToastTypeEnum.Warning => "btn-warning",
        ToastTypeEnum.Info => "btn-info",
        _ => "btn-secondary",
    };

    public void Dispose()
    {
        toastService.UnSubscribe(ShowToast, HideToastById);
        foreach (var timer in toastTimers.Values)
        {
            timer.Dispose();
        }
        toastTimers.Clear();
    }

    private class ToastDisplayModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;
        public ToastTypeEnum ToastType { get; set; }
        public bool IsVisible { get; set; }
    }


}