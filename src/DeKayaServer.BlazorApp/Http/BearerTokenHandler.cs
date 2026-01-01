using DeKayaServer.BlazorApp.Interfaces;
using System.Net.Http.Headers;

namespace DeKayaServer.BlazorApp.Http;

public sealed class BearerTokenHandler(IAccessTokenStoreService tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
