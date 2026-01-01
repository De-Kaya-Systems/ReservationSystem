using System.Text.Json.Serialization;

namespace DeKayaServer.BlazorApp.Models;

public sealed class Result<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errorMessages")]
    public List<string>? ErrorMessages { get; set; }

    [JsonPropertyName("isSuccessful")]
    public bool IsSuccessful { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}
