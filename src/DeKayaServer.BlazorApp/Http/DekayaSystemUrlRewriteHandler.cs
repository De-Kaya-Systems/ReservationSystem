namespace DeKayaServer.BlazorApp.Http;

public sealed class DekayaSystemUrlRewriteHandler : DelegatingHandler
{
    private const string Prefix = "/dekayasystem/";
    private static readonly Uri ApiBaseUri = new("https://localhost:7040/", UriKind.Absolute);

    protected override Task<HttpResponseMessage> SendAsync(
         HttpRequestMessage request,
         CancellationToken cancellationToken)
    {
        if (request.RequestUri is null)
            return base.SendAsync(request, cancellationToken);

        // Only rewrite relative URIs. Absolute URIs are assumed intentional.
        if (request.RequestUri.IsAbsoluteUri)
            return base.SendAsync(request, cancellationToken);

        var raw = request.RequestUri.OriginalString;
        if (string.IsNullOrWhiteSpace(raw))
            return base.SendAsync(request, cancellationToken);

        // Normalize to start with '/'
        if (raw[0] != '/')
            raw = "/" + raw;

        // Only rewrite if it matches our virtual prefix
        if (!raw.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            return base.SendAsync(request, cancellationToken);

        // Strip "/dekayasystem/" prefix; preserve any query string
        var remainder = raw[Prefix.Length..].TrimStart('/');

        request.RequestUri = new Uri(ApiBaseUri, remainder);
        return base.SendAsync(request, cancellationToken);
    }
}
