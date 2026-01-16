using DeKayaServer.BlazorApp.Interfaces;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public sealed class CachedAccessTokenStoreService(IAccessTokenStoreService inner) : IAccessTokenStoreService
{
    private string? token;

    public async ValueTask<string?> GetAsync(CancellationToken cancellation = default)
    {
        if (!string.IsNullOrWhiteSpace(token))
            return token;

        token = await inner.GetAsync(cancellation);
        return token;
    }

    public async ValueTask SetAsync(string accessToken, CancellationToken cancellation = default)
    {
        token = accessToken;
        await inner.SetAsync(accessToken, cancellation);
    }

    public async ValueTask ClearAsync(CancellationToken cancellation = default)
    {
        token = null;
        await inner.ClearAsync(cancellation);
    }
}