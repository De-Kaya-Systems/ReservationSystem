using TS.Result;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IPermissionService
{
    Task<Result<List<string>>> GetAllAsync( CancellationToken ct = default );
    Task<Result<string>> UpdateRolePermissionsAsync(
        Guid roleId,
        List<string> permissions,
        CancellationToken ct = default );
}