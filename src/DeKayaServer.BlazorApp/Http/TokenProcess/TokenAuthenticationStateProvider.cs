using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public sealed class TokenAuthenticationStateProvider(
    IAccessTokenStoreService tokenStoreService,
    CurrentAccessToken currentAccessToken,
    ILogger<TokenAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStoreService.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogDebug("AuthState: no token in storage.");
            currentAccessToken.Value = null;
            return new AuthenticationState(Anonymous);
        }

        currentAccessToken.Value = token;

        var principal = CreatePrincipalOrNull(token);
        if (principal is null)
        {
            logger.LogWarning("AuthState: token exists but is invalid (expired or unreadable). Clearing token.");
            currentAccessToken.Value = null;
            await tokenStoreService.ClearAsync();
            return new AuthenticationState(Anonymous);
        }

        logger.LogDebug("AuthState: authenticated.");
        return new AuthenticationState(principal);
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await tokenStoreService.SetAsync(token);
        currentAccessToken.Value = token;

        var principal = CreatePrincipalOrNull(token) ?? Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        currentAccessToken.Value = null;
        await tokenStoreService.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

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
            return null;
        }

        if (jwt.ValidTo == DateTime.MinValue || jwt.ValidTo < DateTime.UtcNow)
            return null;

        return new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, authenticationType: "jwt"));
    }
}
