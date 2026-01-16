using DeKayaServer.BlazorApp.Http.TokenProcess;
using DeKayaServer.BlazorApp.Models;
using System.Net;
using System.Text.Json;

namespace DeKayaServer.BlazorApp.Http;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default);
    Task<Result<T>> PostAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);
    Task<Result<T>> PutAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);
    Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct = default);
}

public sealed class ApiClient(
    HttpClient http,
    CircuitIdProvider circuitIdProvider,
    IForceLogoutService forceLogoutService) : IApiClient
{
    private const string CircuitHeaderName = "X-Circuit-Id";
    private static readonly JsonSerializerOptions _opt = new() { PropertyNameCaseInsensitive = true };

    public Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), ct);

    public Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, url), ct);

    public Task<Result<T>> PostAsync<TReq, T>(string url, TReq req, CancellationToken ct = default)
        => SendWithJsonAsync<TReq, T>(HttpMethod.Post, url, req, ct);

    public Task<Result<T>> PutAsync<TReq, T>(string url, TReq req, CancellationToken ct = default)
        => SendWithJsonAsync<TReq, T>(HttpMethod.Put, url, req, ct);

    private Task<Result<T>> SendWithJsonAsync<TReq, T>(HttpMethod method, string url, TReq req, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(req)
        };

        return SendAsync<T>(request, ct);
    }

    private async Task<Result<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        using var req = request;

        var circuitId = circuitIdProvider.CircuitId;
        if (!string.IsNullOrWhiteSpace(circuitId))
        {
            req.Headers.Remove(CircuitHeaderName);
            req.Headers.Add(CircuitHeaderName, circuitId);
        }

        try
        {
            using var response = await http.SendAsync(req, ct);

            //Buna daha sonra role hanteringde bakacagim.
            //EN: I must check this later in role handling.
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                await forceLogoutService.ForceLogoutAsync();
            }

            var body = response.Content is null
                ? null
                : await response.Content.ReadAsStringAsync(ct);

            var result = Deserialize<Result<T>>(body) ?? new Result<T>
            {
                IsSuccessful = false,
                StatusCode = (int)response.StatusCode,
                ErrorMessages = ["Empty response from server."]
            };
            return result;
        }
        catch (TaskCanceledException)
        {
            return Failure<T>(408, "İstek zaman aşımına uğradı (timeout).");
        }
        catch (HttpRequestException)
        {
            return Failure<T>(503, "Sunucuya ulaşılamadı. Bağlantını kontrol et.");
        }
    }

    private static TObj? Deserialize<TObj>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<TObj>(json, _opt); }
        catch { return default; }
    }

    private static Result<T> Failure<T>(int status, string message) => new()
    {
        IsSuccessful = false,
        StatusCode = status,
        ErrorMessages = [message]
    };
}