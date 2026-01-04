using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IAuthService
{
    Task LoginAsync(LoginRequest loginRequest, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(string email, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default);
    Task<bool> LogOutAsync();
}
