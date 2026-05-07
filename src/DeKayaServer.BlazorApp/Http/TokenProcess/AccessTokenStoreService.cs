using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

/// <summary>
/// Manages access token persistence using ProtectedLocalStorage.
/// 
/// Handles:
/// - Secure storage in browser's protected local storage
/// - Exception handling for pre-render and JS runtime scenarios
/// - Graceful fallback when storage is unavailable
/// - Comprehensive logging for debugging
/// </summary>
public sealed class AccessTokenStoreService(
    ProtectedLocalStorage storage,
    ILogger<AccessTokenStoreService> logger) : IAccessTokenStoreService
{
    public async ValueTask<string?> GetAsync(CancellationToken cancellation = default)
    {
        try
        {
            var result = await storage.GetAsync<string>(StorageKeyConstants.AccessToken);
            
            if (result.Success)
            {
                logger.LogDebug("AccessTokenStoreService.GetAsync: Token retrieved from storage successfully. Length={Length}", result.Value?.Length ?? 0);
                return result.Value;
            }

            logger.LogDebug("AccessTokenStoreService.GetAsync: No token found in storage (Success=false).");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "AccessTokenStoreService.GetAsync: InvalidOperationException - JS runtime likely not available yet or prerendering. This is expected during initial page load.");
            return null;
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "AccessTokenStoreService.GetAsync: JSDisconnectedException - JavaScript runtime disconnected. Will retry on reconnection.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AccessTokenStoreService.GetAsync: Unexpected exception while retrieving token from storage.");
            return null;
        }
    }

    public async ValueTask SetAsync(string accessToken, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogWarning("AccessTokenStoreService.SetAsync: Attempted to set null or empty token.");
            return;
        }

        try
        {
            await storage.SetAsync(StorageKeyConstants.AccessToken, accessToken);
            logger.LogInformation("AccessTokenStoreService.SetAsync: Token stored successfully. Length={Length}", accessToken.Length);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "AccessTokenStoreService.SetAsync: InvalidOperationException - storage not available (prerender/interactive state). Token will be kept in-memory.");
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "AccessTokenStoreService.SetAsync: JSDisconnectedException - token storage failed but will retry.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AccessTokenStoreService.SetAsync: Unexpected exception while storing token.");
        }
    }

    public async ValueTask ClearAsync(CancellationToken cancellation = default)
    {
        try
        {
            await storage.DeleteAsync(StorageKeyConstants.AccessToken);
            logger.LogInformation("AccessTokenStoreService.ClearAsync: Token cleared from storage successfully.");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "AccessTokenStoreService.ClearAsync: InvalidOperationException while clearing token.");
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "AccessTokenStoreService.ClearAsync: JSDisconnectedException while clearing token.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AccessTokenStoreService.ClearAsync: Unexpected exception while clearing token.");
        }
    }
}
