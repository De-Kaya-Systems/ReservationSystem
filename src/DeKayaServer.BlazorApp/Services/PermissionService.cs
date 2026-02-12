using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public sealed class PermissionService( IApiClient apiClient ) : IPermissionService
{
    public Task<Result<List<string>>> GetAllAsync( CancellationToken ct = default )
        => apiClient.GetAsync<List<string>>( EndpointConstants.Permissions, ct );
}
