using DeKayaServer.BlazorApp.Http.TokenProcess;
using System.Net.Http.Headers;

namespace DeKayaServer.BlazorApp.Http;

public sealed class AuthHeaderHandler(
    CircuitServicesAccessor circuitServicesAccessor) : DelegatingHandler
{
    private const string CircuitHeaderName = "X-Circuit-Id";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues(CircuitHeaderName, out var values))
            return base.SendAsync(request, cancellationToken);

        var circuitId = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(circuitId))
            return base.SendAsync(request, cancellationToken);

        if (!circuitServicesAccessor.TryGetServices(circuitId, out var sp))
            return base.SendAsync(request, cancellationToken);

        var currentAccessToken = sp.GetRequiredService<CurrentAccessToken>();
        if (!string.IsNullOrWhiteSpace(currentAccessToken.Value))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentAccessToken.Value);
        }

        return base.SendAsync(request, cancellationToken);
    }
}