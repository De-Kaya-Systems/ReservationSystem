using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);
    Task LoginAsync(LoginRequest loginRequest, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default);
    Task<bool> LogOutAsync();
}
