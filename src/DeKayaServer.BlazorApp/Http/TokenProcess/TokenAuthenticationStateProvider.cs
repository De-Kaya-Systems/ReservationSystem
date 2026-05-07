using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

/// <summary>
/// Manages authentication state based on JWT token.
/// 
/// Responsibilities:
/// - Provides current authentication state to Blazor
/// - Validates token expiration and integrity
/// - Syncs token between storage and in-memory cache
/// - Handles login/logout state transitions
/// </summary>
public sealed class TokenAuthenticationStateProvider(
    IAccessTokenStoreService tokenStoreService,
    CurrentAccessToken currentAccessToken,
    ILogger<TokenAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await tokenStoreService.GetAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogDebug("TokenAuthenticationStateProvider.GetAuthenticationStateAsync: No token in storage - returning Anonymous state.");
                currentAccessToken.Value = null;
                return new AuthenticationState(Anonymous);
            }

            // Update in-memory cache
            currentAccessToken.Value = token;

            var principal = CreatePrincipalOrNull(token);
            if (principal is null)
            {
                logger.LogWarning("TokenAuthenticationStateProvider.GetAuthenticationStateAsync: Token exists but is invalid (expired or unreadable). Clearing token and returning Anonymous state.");
                currentAccessToken.Value = null;
                await tokenStoreService.ClearAsync();
                return new AuthenticationState(Anonymous);
            }

            logger.LogDebug("TokenAuthenticationStateProvider.GetAuthenticationStateAsync: Successfully authenticated user.");
            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TokenAuthenticationStateProvider.GetAuthenticationStateAsync: Unexpected exception while getting authentication state.");
            currentAccessToken.Value = null;
            return new AuthenticationState(Anonymous);
        }
    }

    /// <summary>
    /// Called after successful login to update authentication state.
    /// Updates both in-memory cache and persistent storage.
    /// </summary>
    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("TokenAuthenticationStateProvider.MarkUserAsAuthenticatedAsync: Token is null or empty.");
            return;
        }

        logger.LogInformation("TokenAuthenticationStateProvider.MarkUserAsAuthenticatedAsync: Marking user as authenticated. Token length={TokenLength}", token.Length);
        
        // Update persistent storage first
        await tokenStoreService.SetAsync(token);
        
        // Update in-memory cache
        currentAccessToken.Value = token;

        var principal = CreatePrincipalOrNull(token) ?? Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    /// <summary>
    /// Called on logout to clear authentication state.
    /// Clears both in-memory cache and persistent storage.
    /// </summary>
    public async Task MarkUserAsLoggedOutAsync()
    {
        logger.LogInformation("TokenAuthenticationStateProvider.MarkUserAsLoggedOutAsync: Marking user as logged out.");
        
        currentAccessToken.Value = null;
        await tokenStoreService.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    /// <summary>
    /// Validates JWT token and creates ClaimsPrincipal if valid.
    /// Returns null if token is invalid, expired, or unreadable.
    /// </summary>
    private static ClaimsPrincipal? CreatePrincipalOrNull(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch
        {
            // Token is malformed or not a valid JWT
            return null;
        }

        // Check if token is expired
        if (jwt.ValidTo == DateTime.MinValue || jwt.ValidTo < DateTime.UtcNow)
            return null;

        return new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, authenticationType: "jwt"));
    }
}
