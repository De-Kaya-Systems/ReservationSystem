namespace DeKayaServer.BlazorApp.Interfaces;

public interface IAccessTokenStoreService
{
    ValueTask<string?> GetAsync(CancellationToken cancellation = default);
    ValueTask SetAsync(string accessToken, CancellationToken cancellation = default);
    ValueTask ClearAsync(CancellationToken cancellation = default);
}
