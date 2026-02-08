using DeKayaServer.BlazorApp.Http;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface IAuthProbeService
{
    Task<Result<string>> PingAuthorizedAsync( CancellationToken ct = default );
}

public sealed class AuthProbeService( IApiClient apiClient ) : IAuthProbeService
{
    public Task<Result<string>> PingAuthorizedAsync( CancellationToken ct = default )
        => apiClient.GetAsync<string>( "/auth/probe", ct );
}
