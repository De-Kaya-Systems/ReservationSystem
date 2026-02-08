using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Http.TokenProcess;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AuthService(
    IApiClient apiClient,
    TokenAuthenticationStateProvider authStateProvider,
    CurrentAccessToken currentAccessToken ) : IAuthService
{
    public async Task<Result<string>> LoginAsync( LoginRequest loginRequest, CancellationToken ct = default )
    {
        var result = await apiClient.PostAsync<LoginRequest, string>( EndpointConstants.LoginEndpoint, loginRequest, ct );

        if ( result.IsSuccessful && !string.IsNullOrWhiteSpace( result.Data ) )
        {
            currentAccessToken.Value = result.Data;
            await authStateProvider.MarkUserAsAuthenticatedAsync( result.Data );
        }

        return result;
    }

    public Task<Result<string>> ForgotPasswordAsync( string email, CancellationToken ct = default )
        => apiClient.PostAsync<object, string>(
            $"{EndpointConstants.ForgotPasswordEndpoint}/{email}",
            new { },
            ct );

    public Task<Result<string>> ResetPasswordAsync( string forgotPasswordCode, string newPassword, bool logoutAllDevices, CancellationToken ct = default )
        => apiClient.PostAsync<object, string>(
            EndpointConstants.ResetPasswordEndpoint,
            new { forgotPasswordCode, newPassword, logoutAllDevices },
            ct );

    public Task<Result<bool>> CheckForgotPasswordCodeAsync( string forgotPasswordCode, CancellationToken ct = default )
        => apiClient.GetAsync<bool>(
            $"{EndpointConstants.CheckForgotPasswordCodeEndpoint}/{forgotPasswordCode}",
            ct );

    public async Task<bool> LogOutAsync()
    {
        currentAccessToken.Value = null;
        await authStateProvider.MarkUserAsLoggedOutAsync();
        return true;
    }
}
