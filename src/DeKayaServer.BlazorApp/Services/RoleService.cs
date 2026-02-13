using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.Contracts.Roles;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public sealed class RoleService( IApiClient apiClient ) : IRoleService
{
    public Task<Result<string>> CreateAsync( string name, bool isActive, CancellationToken cancellationToken = default )
        => apiClient.PostAsync<CreateRoleRequest, string>(
            EndpointConstants.Roles,
            new CreateRoleRequest( name, isActive ),
            cancellationToken );

    public Task<Result<string>> UpdateAsync( Guid id, string name, bool isActive, CancellationToken cancellationToken = default )
        => apiClient.PutAsync<UpdateRoleRequest, string>(
            EndpointConstants.Roles,
            new UpdateRoleRequest( id, name, isActive ),
            cancellationToken );

    public async Task<Result<List<RoleDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<RoleDto>>( EndpointConstants.ODataRoles, cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<RoleDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<RoleDto>>
        {
            IsSuccessful = true,
            StatusCode = odataRes.StatusCode,
            Data = odataRes.Data.Value ?? []
        };
    }

    public Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.DeleteAsync<string>( $"{EndpointConstants.Roles}/{id}", cancellationToken );

    public Task<Result<RoleDto>> GetByPermissionIdAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.GetAsync<RoleDto>( $"{EndpointConstants.Roles}/{id}", cancellationToken );

    private sealed class ODataEnvelope<T>
    {
        public List<T> Value { get; set; } = [];
    }
}
