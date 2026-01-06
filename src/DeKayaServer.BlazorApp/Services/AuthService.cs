using DeKayaServer.BlazorApp.Constans;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AuthService(
    IApiClient apiClient,
    IAccessTokenStoreService localStorage
) : IAuthService
{
    //public Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    //    => apiClient.PostAsync<LoginRequest, string>(EndpointConstans.LoginEndpoint, loginRequest, cancellationToken);

    public Task LoginAsync(LoginRequest loginRequest, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default)
    {
        return apiClient.PostAsync<LoginRequest, string>(
            EndpointConstants.LoginEndpoint,
            loginRequest,
            onSuccess,
            onError,
            cancellationToken);
    }

    public Task ForgotPasswordAsync(string email, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default)
    {
        return apiClient.PostAsync<object, string>(
            $"{EndpointConstants.ForgotPasswordEndpoint}/{email}",
            new { },
            onSuccess,
            onError,
            cancellationToken);
    }

    public Task ResetPasswordAsync(string forgotPasswordCode, string newPassword, Action<string> onSuccess, Action<Result<string>>? onError = null, CancellationToken cancellationToken = default)
    {
        return apiClient.PostAsync<object, string>(
            EndpointConstants.ResetPasswordEndpoint,
            new { forgotPasswordCode, newPassword },
            onSuccess,
            onError,
            cancellationToken);
    }

    public Task CheckForgotPasswordCodeAsync(string forgotPasswordCode, Action<bool> onSuccess, Action<Result<bool>>? onError = null, CancellationToken cancellationToken = default)
    {
        return apiClient.GetAsync<bool>(
            $"{EndpointConstants.CheckForgotPasswordCodeEndpoint}/{forgotPasswordCode}",
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
