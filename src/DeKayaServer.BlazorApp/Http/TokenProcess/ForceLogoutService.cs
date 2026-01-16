using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Services;
using Microsoft.AspNetCore.Components;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public interface IForceLogoutService
{
    Task ForceLogoutAsync();
}

public sealed class ForceLogoutService(
    TokenAuthenticationStateProvider authStateProvider,
    NavigationManager navigationManager,
    ToastService toastService) : IForceLogoutService
{
    public async Task ForceLogoutAsync()
    {
        await authStateProvider.MarkUserAsLoggedOutAsync();
        navigationManager.NavigateTo(EndpointConstants.LoginPage);
        toastService.ShowWarning("Tekrar giriş yapmalısın.");
    }
}