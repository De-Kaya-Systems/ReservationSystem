using DeKayaServer.BlazorApp.Models;
using DeKayaServer.BlazorApp.Services;
using System.Text.Json;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default);

    Task<Result<T>> PostAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);

    Task<Result<T>> PutAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);

    Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct = default);
}

public sealed class ApiClient(HttpClient http, ToastService toast) : IApiClient
{
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
        HttpResponseMessage response;

        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (TaskCanceledException)
        {
            toast.ShowError("İstek zaman aşımına uğradı (timeout).");
            return Failure<T>(408, "Timeout");
        }
        catch (HttpRequestException)
        {
            toast.ShowError("Sunucuya ulaşılamadı. Bağlantını kontrol et.");
            return Failure<T>(503, "Connection error");
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

        // Angular V1 birebir: 403/422/500 ise toast bas
        if (result.StatusCode is 403 or 422 or 500)
        {
            if (result.ErrorMessages is { Count: > 0 })
            {
                foreach (var msg in result.ErrorMessages.Where(m => !string.IsNullOrWhiteSpace(m)))
                    toast.ShowError(msg);
            }
            else
            {
                toast.ShowError("Beklenmeyen bir hata oluştu.");
            }
        }

        return result;
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
