using DeKayaServer.BlazorApp.Models;
using DeKayaServer.BlazorApp.Services;
using System.Text.Json;

namespace DeKayaServer.BlazorApp.Http;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default);
    Task<Result<T>> PostAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);
    Task<Result<T>> PutAsync<TReq, T>(string url, TReq req, CancellationToken ct = default);
    Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct = default);

    Task GetAsync<T>(
       string url,
       Action<T> onSuccess,
       Action<Result<T>>? onError = null,
       CancellationToken ct = default);

    Task PostAsync<TReq, T>(
        string url,
        TReq req,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default);

    Task PutAsync<TReq, T>(
        string url,
        TReq req,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default);

    Task DeleteAsync<T>(
        string url,
        Action<T> onSuccess,
        Action<Result<T>>? onError = null,
        CancellationToken ct = default);
}

public sealed class ApiClient(HttpClient http, ToastService toast) : IApiClient
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

        if (!result.IsSuccessful)
        {
            HandleErrorToast(result);
        }

        return result;
    }

    /// <summary>
    /// Hata durumunda kullanıcıya toast mesajı gösterir.
    /// Önce backend'den gelen mesajı kontrol eder, yoksa status code'a göre genel mesaj gösterir.
    /// EN: Shows toast message to user on error.
    /// First checks backend message, if not available shows general message based on status code.
    /// </summary>
    private void HandleErrorToast<T>(Result<T> result)
    {
        //Backendden gelen hata mesajları
        //EN: Error messages from backend
        var errorMessages = result.ErrorMessages?
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        // Backend de mesaj varsa goster
        // EN: If backend has message, show it
        if (errorMessages is { Count: > 0 })
        {
            //Sadece ilk mesajı göster
            //EN: Show only the first message
            toast.ShowError(errorMessages.First());
            return;
        }

        // Backend mesajı yoksa status code'a göre genel mesaj göster
        // EN: If no backend message, show general message based on status code
        var fallbackMessage = result.StatusCode switch
        {
            400 => "Geçersiz istek yapıldı.", // Bad Request
            401 => "Yetkisiz erişim. Lütfen giriş yapın.", // Unauthorized
            403 => "Erişim engellendi.", // Forbidden
            404 => "İstenen kaynak bulunamadı.", // Not Found
            408 => "İstek zaman aşımına uğradı.", // Request Timeout
            422 => "İstek tamamlanmadı. Girdiğiniz verileri kontrol edin", // Unprocessable Entity
            500 => "Sunucu hatası oluştu.", // Internal Server Error
            503 => "Sunucu hizmet veremiyor.", // Service Unavailable
            _ => $"Bir hata oluştu. Hata kodu : {result.StatusCode}" // Unknown error
        };
        toast.ShowError(fallbackMessage);
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
