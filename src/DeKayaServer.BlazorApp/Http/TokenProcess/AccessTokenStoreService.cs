using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public sealed class AccessTokenStoreService(
    ProtectedLocalStorage storage,
    ILogger<AccessTokenStoreService> logger) : IAccessTokenStoreService
{
    public async ValueTask<string?> GetAsync(CancellationToken cancellation = default)
    {
        try
        {
            var result = await storage.GetAsync<string>(StorageKeyConstants.AccessToken);
            logger.LogDebug("GetAsync from storage: Success={Success}", result.Success);
            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "GetAsync failed: InvalidOperationException (likely JS runtime not available yet).");
            return null;
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "GetAsync failed: JSDisconnectedException.");
            return null;
        }
    }

    public async ValueTask SetAsync(string accessToken, CancellationToken cancellation = default)
    {
        try
        {
            await storage.SetAsync(StorageKeyConstants.AccessToken, accessToken);
            logger.LogDebug("SetAsync to storage: OK (len={Len}).", accessToken?.Length ?? 0);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "SetAsync failed: InvalidOperationException (likely prerender/not interactive yet).");
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "SetAsync failed: JSDisconnectedException.");
        }
    }

    public async ValueTask ClearAsync(CancellationToken cancellation = default)
    {
        try
        {
            await storage.DeleteAsync(StorageKeyConstants.AccessToken);
            logger.LogDebug("ClearAsync: deleted from storage.");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "ClearAsync failed: InvalidOperationException.");
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "ClearAsync failed: JSDisconnectedException.");
        }
    }
}
