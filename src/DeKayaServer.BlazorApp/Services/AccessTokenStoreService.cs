using DeKayaServer.BlazorApp.Constans;
using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AccessTokenStoreService(ProtectedLocalStorage storage) : IAccessTokenStoreService
{
    public async ValueTask<string?> GetAsync(CancellationToken cancellation = default)
    {
        var result = await storage.GetAsync<string>(StorageKeyConstants.AccessToken);
        return result.Success ? result.Value : null;
    }

    public async ValueTask SetAsync(string accessToken, CancellationToken cancellation = default)
    {
        await storage.SetAsync(StorageKeyConstants.AccessToken, accessToken);
    }

    public async ValueTask ClearAsync(CancellationToken cancellation = default)
    {
        await storage.DeleteAsync(StorageKeyConstants.AccessToken);
    }
}
