using DeKayaServer.BlazorApp.Http.TokenProcess;
using System.Net.Http.Headers;

namespace DeKayaServer.BlazorApp.Http;

/// <summary>
/// Adds Bearer token to HTTP requests.
/// 
/// Strategy:
/// 1. First tries to get token from CurrentAccessToken (in-memory cache, fastest)
/// 2. Falls back to service provider + CircuitId (for circuit-scoped cases)
/// 3. Token is added to every outgoing request
/// 
/// This ensures token is sent in Azure and other production environments
/// where CircuitId may not always be available.
/// </summary>
public sealed class AuthHeaderHandler(
    CurrentAccessToken currentAccessToken,
    CircuitServicesAccessor circuitServicesAccessor,
    ILogger<AuthHeaderHandler> logger) : DelegatingHandler
{
    private const string CircuitHeaderName = "X-Circuit-Id";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = GetToken(request);
        
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            logger.LogDebug("AuthHeaderHandler: Bearer token added to request.");
        }
        else
        {
            logger.LogDebug("AuthHeaderHandler: No token available for request.");
        }

        return base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets token from multiple sources with fallback strategy.
    /// 
    /// Priority:
    /// 1. CurrentAccessToken (in-memory, always available if user logged in)
    /// 2. Service provider via CircuitId (fallback for circuit-scoped DI)
    /// </summary>
    private string? GetToken(HttpRequestMessage request)
    {
        // First, try the in-memory token (fastest path - works in Azure)
        if (!string.IsNullOrWhiteSpace(currentAccessToken.Value))
        {
            return currentAccessToken.Value;
        }

        // Fallback: try to get token via circuit (local Blazor scenario)
        if (!request.Headers.TryGetValues(CircuitHeaderName, out var values))
        {
            return null;
        }

        var circuitId = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(circuitId))
        {
            return null;
        }

        if (!circuitServicesAccessor.TryGetServices(circuitId, out var sp))
        {
            return null;
        }

        var token = sp.GetRequiredService<CurrentAccessToken>().Value;
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }
}