using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IAuthService
{
    Task LoginAsync(LoginRequest loginRequest, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(string email, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string forgotPasswordCode, string newPassword, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default);
    Task CheckForgotPasswordCodeAsync(string forgotPasswordCode, Action<bool> onSuccess, Action<Result<bool>>? onError = null, CancellationToken cancellationToken = default);
    Task<bool> LogOutAsync();
}
