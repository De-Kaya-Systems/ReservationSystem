using DeKayaServer.BlazorApp.Constants;

namespace DeKayaServer.BlazorApp.Services;

public class ToastService
{
    private readonly List<Action<ToastInput>> _onShowSubscribers = new();
    private readonly List<Action<string>> _onHideSubscribers = new();

    public void Subscribe(Action<ToastInput> onShow, Action<string>? onHide = null)
    {
        _onShowSubscribers.Add(onShow);
        if (onHide != null)
        {
            _onHideSubscribers.Add(onHide);
        }
    }

    public void UnSubscribe(Action<ToastInput> onShow, Action<string>? onHide = null)
    {
        _onShowSubscribers.Remove(onShow);
        if (onHide != null)
        {
            _onHideSubscribers.Remove(onHide);
        }
    }

    public void ShowToast(ToastInput input)
    {
        foreach (var subscriber in _onShowSubscribers.ToList())
        {
            subscriber.Invoke(input);
        }
    }

    public void ShowSuccess(string? message = ToastMessageConstants.Success)
    {
        ShowToast(new ToastInput(ToastTitles.Success, message!, ToastTypeEnum.Success));
    }

    public void ShowError(string? message = ToastMessageConstants.Error)
    {
        ShowToast(new ToastInput(ToastTitles.Error, message!, ToastTypeEnum.Error));
    }

    public void ShowWarning(string? message = ToastMessageConstants.Warning)
    {
        ShowToast(new ToastInput(ToastTitles.Warning, message!, ToastTypeEnum.Warning));
    }
    public void ShowInfo(string? message = ToastMessageConstants.Info)
    {
        ShowToast(new ToastInput(ToastTitles.Info, message!, ToastTypeEnum.Info));
    }

    public void HideToast(string toastId)
    {
        foreach (var subscriber in _onHideSubscribers.ToList())
        {
            subscriber.Invoke(toastId);
        }
    }
}

public record ToastInput(string Title, string Message, ToastTypeEnum ToastType, int Duration = 5000)
{
    public string Id { get; } = Guid.NewGuid().ToString();

}

public enum ToastTypeEnum
{
    Success,
    Error,
    Warning,
    Info
}

public static class ToastTitles
{
    public const string Success = "Success";
    public const string Error = "Error";
    public const string Warning = "Warning";
    public const string Info = "Info";
}