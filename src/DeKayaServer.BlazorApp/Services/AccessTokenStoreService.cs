using DeKayaServer.BlazorApp.Constans;
using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AccessTokenStoreService(ProtectedLocalStorage storage) : IAccessTokenStoreService
{
    private string? cached;

    public async ValueTask<string?> GetAsync(CancellationToken cancellation = default)
    {
        if (!string.IsNullOrWhiteSpace(cached))
            return cached;

        var result = await storage.GetAsync<string>(StorageKeyConstants.AccessToken);
        cached = result.Success ? result.Value : null;
        return cached;
    }

    public async ValueTask SetAsync(string accessToken, CancellationToken cancellation = default)
    {
        cached = accessToken;
        await storage.SetAsync(StorageKeyConstants.AccessToken, accessToken);
    }

    public async ValueTask ClearAsync(CancellationToken cancellation)
    {
        cached = null;
        await storage.DeleteAsync(StorageKeyConstants.AccessToken);
    }

}
