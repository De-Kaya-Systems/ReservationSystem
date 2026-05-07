using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Http.TokenProcess;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

/// <summary>
/// Handles authentication operations.
/// 
/// Responsibilities:
/// - Communicates with backend auth API
/// - Manages token lifecycle (login/logout)
/// - Syncs authentication state with backend
/// - Provides comprehensive error handling
/// </summary>
public sealed class AuthService(
    IApiClient apiClient,
    TokenAuthenticationStateProvider authStateProvider,
    CurrentAccessToken currentAccessToken,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken ct = default)
    {
        if (loginRequest is null || string.IsNullOrWhiteSpace(loginRequest.EmailOrUserName))
        {
            logger.LogWarning("AuthService.LoginAsync: Invalid login request (null or empty credentials).");
            return Result<string>.Failure("Invalid login request.");
        }

        logger.LogInformation("AuthService.LoginAsync: Attempting login for user: {EmailOrUserName}", loginRequest.EmailOrUserName);

        var result = await apiClient.PostAsync<LoginRequest, string>(
            EndpointConstants.LoginEndpoint, 
            loginRequest, 
            ct);

        if (!result.IsSuccessful)
        {
            logger.LogWarning("AuthService.LoginAsync: Login failed - {ErrorMessage}", string.Join("; ", result.ErrorMessages ?? []));
            return result;
        }

        if (string.IsNullOrWhiteSpace(result.Data))
        {
            logger.LogError("AuthService.LoginAsync: Backend returned success but token is empty!");
            return Result<string>.Failure("Server returned empty token. Please try again.");
        }

        try
        {
            logger.LogDebug("AuthService.LoginAsync: Setting token in-memory cache and authentication state.");
            
            // Update in-memory token
            currentAccessToken.Value = result.Data;
            
            // Update authentication state (will also update persistent storage)
            await authStateProvider.MarkUserAsAuthenticatedAsync(result.Data);
            
            logger.LogInformation("AuthService.LoginAsync: Login successful. Token stored. Token length={TokenLength}", result.Data.Length);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AuthService.LoginAsync: Exception while updating authentication state after login.");
            return Result<string>.Failure("Login succeeded but failed to update local state. Please refresh the page.");
        }
    }

    public Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("AuthService.ForgotPasswordAsync: Email is empty.");
            return Task.FromResult(Result<string>.Failure("Email is required."));
        }

        logger.LogInformation("AuthService.ForgotPasswordAsync: Requesting password reset for email: {Email}", email);

        return apiClient.PostAsync<object, string>(
            $"{EndpointConstants.ForgotPasswordEndpoint}/{email}",
            new { },
            ct);
    }

    public Task<Result<string>> ResetPasswordAsync(
        string forgotPasswordCode, 
        string newPassword, 
        bool logoutAllDevices, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(forgotPasswordCode) || string.IsNullOrWhiteSpace(newPassword))
        {
            logger.LogWarning("AuthService.ResetPasswordAsync: Invalid parameters.");
            return Task.FromResult(Result<string>.Failure("Invalid reset parameters."));
        }

        logger.LogInformation("AuthService.ResetPasswordAsync: Resetting password with logout all devices = {LogoutAll}", logoutAllDevices);

        return apiClient.PostAsync<object, string>(
            EndpointConstants.ResetPasswordEndpoint,
            new { forgotPasswordCode, newPassword, logoutAllDevices },
            ct);
    }

    public Task<Result<bool>> CheckForgotPasswordCodeAsync(string forgotPasswordCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(forgotPasswordCode))
        {
            logger.LogWarning("AuthService.CheckForgotPasswordCodeAsync: Code is empty.");
            return Task.FromResult(Result<bool>.Failure("Code is required."));
        }

        return apiClient.GetAsync<bool>(
            $"{EndpointConstants.CheckForgotPasswordCodeEndpoint}/{forgotPasswordCode}",
            ct);
    }

    public async Task<bool> LogOutAsync()
    {
        logger.LogInformation("AuthService.LogOutAsync: Logging out user.");
        
        try
        {
            currentAccessToken.Value = null;
            await authStateProvider.MarkUserAsLoggedOutAsync();
            
            logger.LogInformation("AuthService.LogOutAsync: Logout successful.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AuthService.LogOutAsync: Exception during logout.");
            return false;
        }
    }
}
