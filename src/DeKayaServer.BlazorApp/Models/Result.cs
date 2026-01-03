namespace DeKayaServer.BlazorApp.Models;

//public sealed class Result<T>
//{
//    [JsonPropertyName("data")]
//    public T? Data { get; set; }

//    [JsonPropertyName("errorMessages")]
//    public List<string>? ErrorMessages { get; set; }

//    [JsonPropertyName("isSuccessful")]
//    public bool IsSuccessful { get; set; }

//    [JsonPropertyName("statusCode")]
//    public int StatusCode { get; set; }
//}

public abstract record Result
{
    public bool IsSuccessful { get; init; }
    public int StatusCode { get; init; }
    public List<string>? ErrorMessages { get; init; }
}

public sealed record Result<T> : Result
{
    public T? Data { get; init; }
}