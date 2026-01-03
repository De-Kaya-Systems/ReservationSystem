using DeKayaServer.BlazorApp.Constans;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AuthService(
    IApiClient apiClient,
    IAccessTokenStoreService localStorage
) : IAuthService
{
    public Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<LoginRequest, string>(EndpointConstans.LoginEndpoint, loginRequest, cancellationToken);

    public Task LoginAsync(LoginRequest loginRequest, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default)
    {
        return apiClient.PostAsync<LoginRequest, string>(
            EndpointConstans.LoginEndpoint,
            loginRequest,
            onSuccess,
            onError,
            cancellationToken);
    }
    public async Task<bool> LogOutAsync()
    {
        await localStorage.ClearAsync();
        return true;
    }
}
