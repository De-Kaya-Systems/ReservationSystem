using DeKayaServer.BlazorApp.Http.TokenProcess;
using DeKayaServer.BlazorApp.Models;
using System.Net;
using System.Text.Json;

namespace DeKayaServer.BlazorApp.Http;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>( string url, CancellationToken ct = default );
    Task<Result<T>> PostAsync<TReq, T>( string url, TReq req, CancellationToken ct = default );
    Task<Result<T>> PutAsync<TReq, T>( string url, TReq req, CancellationToken ct = default );
    Task<Result<T>> DeleteAsync<T>( string url, CancellationToken ct = default );

    Task<Result<T>> GetRawAsync<T>( string url, CancellationToken ct = default );
}

public sealed class ApiClient(
    HttpClient http,
    CircuitIdProvider circuitIdProvider,
    IForceLogoutService forceLogoutService ) : IApiClient
{
    private const string CircuitHeaderName = "X-Circuit-Id";
    private static readonly JsonSerializerOptions _opt = new() { PropertyNameCaseInsensitive = true };

    public Task<Result<T>> GetAsync<T>( string url, CancellationToken ct = default )
        => SendAsync<T>( new HttpRequestMessage( HttpMethod.Get, url ), ct, expectResultEnvelope: true );

    public Task<Result<T>> GetRawAsync<T>( string url, CancellationToken ct = default )
        => SendAsync<T>( new HttpRequestMessage( HttpMethod.Get, url ), ct, expectResultEnvelope: false );

    public Task<Result<T>> DeleteAsync<T>( string url, CancellationToken ct = default )
        => SendAsync<T>( new HttpRequestMessage( HttpMethod.Delete, url ), ct, expectResultEnvelope: true );

    public Task<Result<T>> PostAsync<TReq, T>( string url, TReq req, CancellationToken ct = default )
        => SendWithJsonAsync<TReq, T>( HttpMethod.Post, url, req, ct );

    public Task<Result<T>> PutAsync<TReq, T>( string url, TReq req, CancellationToken ct = default )
        => SendWithJsonAsync<TReq, T>( HttpMethod.Put, url, req, ct );

    private Task<Result<T>> SendWithJsonAsync<TReq, T>( HttpMethod method, string url, TReq req, CancellationToken ct )
    {
        var request = new HttpRequestMessage( method, url )
        {
            Content = JsonContent.Create( req )
        };

        return SendAsync<T>( request, ct, expectResultEnvelope: true );
    }

    private async Task<Result<T>> SendAsync<T>( HttpRequestMessage request, CancellationToken ct, bool expectResultEnvelope )
    {
        using var req = request;

        var circuitId = circuitIdProvider.CircuitId;
        if ( !string.IsNullOrWhiteSpace( circuitId ) )
        {
            req.Headers.Remove( CircuitHeaderName );
            req.Headers.Add( CircuitHeaderName, circuitId );
        }

        try
        {
            using var response = await http.SendAsync( req, ct );

            if ( response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden )
            {
                await forceLogoutService.ForceLogoutAsync();
            }

            var body = response.Content is null
                ? null
                : await response.Content.ReadAsStringAsync( ct );

            if ( !response.IsSuccessStatusCode )
            {
                return Failure<T>( ( int )response.StatusCode, body ?? "Request failed." );
            }

            if ( string.IsNullOrWhiteSpace( body ) )
            {
                return Failure<T>( ( int )response.StatusCode, "Empty response from server." );
            }

            if ( expectResultEnvelope )
            {
                var envelope = Deserialize<Result<T>>( body );
                return envelope ?? Failure<T>( ( int )response.StatusCode, "Invalid response envelope." );
            }

            var data = Deserialize<T>( body );
            if ( data is null )
            {
                return Failure<T>( ( int )response.StatusCode, "Invalid response payload." );
            }

            return new Result<T>
            {
                IsSuccessful = true,
                StatusCode = ( int )response.StatusCode,
                Data = data
            };
        }
        catch ( TaskCanceledException )
        {
            return Failure<T>( 408, "İstek zaman aşımına uğradı (timeout)." );
        }
        catch ( HttpRequestException )
        {
            return Failure<T>( 503, "Sunucuya ulaşılamadı. Bağlantını kontrol et." );
        }
    }

    private static TObj? Deserialize<TObj>( string? json )
    {
        if ( string.IsNullOrWhiteSpace( json ) ) return default;
        try { return JsonSerializer.Deserialize<TObj>( json, _opt ); }
        catch { return default; }
    }

    private static Result<T> Failure<T>( int status, string message ) => new()
    {
        IsSuccessful = false,
        StatusCode = status,
        ErrorMessages = [ message ]
    };
}