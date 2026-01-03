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

    public async Task<bool> LogOutAsync()
    {
        await localStorage.ClearAsync();
        return true;
    }
}
