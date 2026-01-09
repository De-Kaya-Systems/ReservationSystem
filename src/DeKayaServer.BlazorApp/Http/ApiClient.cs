using DeKayaServer.BlazorApp.Models;
using System.Text.Json;

namespace DeKayaServer.BlazorApp.Http;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default);
    Task<Result<T>> PostAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);
    Task<Result<T>> PutAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);
    Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct = default);
}

public sealed class ApiClient(HttpClient http) : IApiClient
{
    private static readonly JsonSerializerOptions _opt = new() { PropertyNameCaseInsensitive = true };
    #region Old methods
    public Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), ct);

    public Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, url), ct);

    public Task<Result<T>> PostAsync<TReq, T>(string url, TReq req, CancellationToken ct = default)
        => SendWithJsonAsync<TReq, T>(HttpMethod.Post, url, req, ct);

    public Task<Result<T>> PutAsync<TReq, T>(string url, TReq req, CancellationToken ct = default)
        => SendWithJsonAsync<TReq, T>(HttpMethod.Put, url, req, ct);
    #endregion

    #region CallBack methods
    public async Task GetAsync<T>(
        string url,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default)
    {
        var result = await GetAsync<T>(url, ct);
        HandleCallbacks(result, onSuccess, onError);
    }

    public async Task PostAsync<TReq, T>(
        string url,
        TReq req,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default)
    {
        var result = await PostAsync<TReq, T>(url, req, ct);
        HandleCallbacks(result, onSuccess, onError);
    }

    public async Task PutAsync<TReq, T>(
        string url,
        TReq req,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default)
    {
        var result = await PutAsync<TReq, T>(url, req, ct);
        HandleCallbacks(result, onSuccess, onError);
    }

    public async Task DeleteAsync<T>(
        string url,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default)
    {
        var result = await DeleteAsync<T>(url, ct);
        HandleCallbacks(result, onSuccess, onError);
    }


    /// <summary>
    /// Callbackleri yönetir
    /// EN: Handles the callbacks
    /// </summary>

    private void HandleCallbacks<T>(Result<T> result, Action<T> onSuccess, Action<Result<T>>? onError)
    {
        if (result.IsSuccessful && result.Data is not null)
        {
            onSuccess(result.Data);
        }
        else if (!result.IsSuccessful && onError is not null)
        {
            onError(result);
        }
    }
    #endregion

    #region Core Http Logic
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

        try
        {
            using var response = await http.SendAsync(req, ct);

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

    #endregion

    #region Helper Methods
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
    #endregion
}
