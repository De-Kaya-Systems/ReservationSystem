using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AuthService(
    IApiClient apiClient,
    IAccessTokenStoreService localStorage
) : IAuthService
{
    public Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken ct = default)
        => apiClient.PostAsync<LoginRequest, string>(EndpointConstants.LoginEndpoint, loginRequest, ct);

    public Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken ct = default)
        => apiClient.PostAsync<object, string>(
            $"{EndpointConstants.ForgotPasswordEndpoint}/{email}",
            new { },
            ct);

    public Task<Result<string>> ResetPasswordAsync(string forgotPasswordCode, string newPassword, CancellationToken ct = default)
       => apiClient.PostAsync<object, string>(
           EndpointConstants.ResetPasswordEndpoint,
           new { forgotPasswordCode, newPassword },
           ct);

    public Task<Result<bool>> CheckForgotPasswordCodeAsync(string forgotPasswordCode, CancellationToken ct = default)
      => apiClient.GetAsync<bool>(
          $"{EndpointConstants.CheckForgotPasswordCodeEndpoint}/{forgotPasswordCode}",
          ct);
    public async Task<bool> LogOutAsync()
    {
        await localStorage.ClearAsync();
        return true;
    }
}
