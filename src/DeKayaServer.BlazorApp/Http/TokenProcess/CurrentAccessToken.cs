namespace DeKayaServer.BlazorApp.Http.TokenProcess;

/// <summary>
/// Thread-safe holder for the current access token in memory.
/// Used by handlers to quickly access token without repeated storage access.
/// </summary>
public sealed class CurrentAccessToken
{
    private string? _value;
    private readonly object _lock = new();

    public string? Value
    {
        get
        {
            lock (_lock)
            {
                return _value;
            }
        }
        set
        {
            lock (_lock)
            {
                _value = value;
            }
        }
    }
}
