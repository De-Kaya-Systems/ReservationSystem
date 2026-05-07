using DeKayaServer.BlazorApp.Interfaces;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

/// <summary>
/// Caching layer for AccessTokenStoreService.
/// 
/// Benefits:
/// - Reduces repeated async calls to protected storage
/// - Faster access for frequently used token
/// - Decorates the underlying storage service
/// - Thread-safe via lock in CurrentAccessToken
/// </summary>
public sealed class CachedAccessTokenStoreService(
    IAccessTokenStoreService inner,
    ILogger<CachedAccessTokenStoreService> logger) : IAccessTokenStoreService
{
    private string? _cachedToken;
    private readonly object _lock = new();

    public async ValueTask<string?> GetAsync(CancellationToken cancellation = default)
    {
        lock (_lock)
        {
            if (!string.IsNullOrWhiteSpace(_cachedToken))
            {
                logger.LogDebug("CachedAccessTokenStoreService.GetAsync: Returning cached token (length={Length}).", _cachedToken.Length);
                return _cachedToken;
            }
        }

        _cachedToken = await inner.GetAsync(cancellation);
        
        if (!string.IsNullOrWhiteSpace(_cachedToken))
        {
            logger.LogDebug("CachedAccessTokenStoreService.GetAsync: Retrieved token from storage and cached it (length={Length}).", _cachedToken.Length);
        }
        else
        {
            logger.LogDebug("CachedAccessTokenStoreService.GetAsync: No token in storage.");
        }

        return _cachedToken;
    }

    public async ValueTask SetAsync(string accessToken, CancellationToken cancellation = default)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            lock (_lock)
            {
                _cachedToken = accessToken;
            }

            logger.LogDebug("CachedAccessTokenStoreService.SetAsync: Cached token and storing to underlying storage (length={Length}).", accessToken.Length);
        }
        
        await inner.SetAsync(accessToken, cancellation);
    }

    public async ValueTask ClearAsync(CancellationToken cancellation = default)
    {
        lock (_lock)
        {
            _cachedToken = null;
        }

        logger.LogDebug("CachedAccessTokenStoreService.ClearAsync: Cleared cache and underlying storage.");
        await inner.ClearAsync(cancellation);
    }
}