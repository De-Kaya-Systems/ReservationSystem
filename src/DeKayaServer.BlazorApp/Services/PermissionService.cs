using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public sealed class PermissionService( IApiClient apiClient ) : IPermissionService
{
    public Task<Result<List<string>>> GetAllAsync( CancellationToken ct = default )
        => apiClient.GetAsync<List<string>>( EndpointConstants.Permissions, ct );

    public Task<Result<string>> UpdateRolePermissionsAsync(
        Guid roleId,
        List<string> permissions,
        CancellationToken ct = default )
        => apiClient.PutAsync<UpdateRolePermissionsRequest, string>(
            EndpointConstants.UpdateRolePermissions,
            new UpdateRolePermissionsRequest( roleId, permissions ),
            ct );

    private sealed record UpdateRolePermissionsRequest( Guid RoleId, List<string> Permissions );
}
