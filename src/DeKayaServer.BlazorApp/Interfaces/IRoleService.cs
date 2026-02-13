using DeKayaServer.Contracts.Roles;
using TS.Result;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IRoleService
{
    Task<Result<string>> CreateAsync( string name, bool isActive, CancellationToken cancellationToken = default );
    Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<List<RoleDto>>> GetAllAsync( CancellationToken cancellationToken = default );
    Task<Result<RoleDto>> GetByPermissionIdAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<string>> UpdateAsync( Guid id, string name, bool isActive, CancellationToken cancellationToken = default );
}