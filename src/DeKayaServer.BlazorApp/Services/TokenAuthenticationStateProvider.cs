using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DeKayaServer.BlazorApp.Services;

public sealed class TokenAuthenticationStateProvider(IAccessTokenStoreService tokenStoreService) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStoreService.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(Anonymous);

        var principal = CreatePrincipalOrNull(token);
        if (principal is null)
        {
            await tokenStoreService.ClearAsync();
            return new AuthenticationState(Anonymous);
        }

        return new AuthenticationState(principal);
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await tokenStoreService.SetAsync(token);

        var principal = CreatePrincipalOrNull(token) ?? Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
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

        var result = new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, authenticationType: "jwt"));
        return result;
    }
}
