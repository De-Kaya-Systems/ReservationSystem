using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Services;

public sealed class RoleService( IApiClient apiClient )
{
    public Task<Result<string>> CreateAsync( string name, bool isActive, CancellationToken cancellationToken = default )
        => apiClient.PostAsync<CreateRoleRequest, string>(
            EndpointConstants.Roles,
            new CreateRoleRequest( name, isActive ),
            cancellationToken );

    public async Task<Result<List<RoleItemDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<RoleItemDto>>( "odata/roles", cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<RoleItemDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<RoleItemDto>>
        {
            IsSuccessful = true,
            StatusCode = odataRes.StatusCode,
            Data = odataRes.Data.Value ?? []
        };
    }
    private sealed record CreateRoleRequest( string Name, bool IsActive );

    private sealed class ODataEnvelope<T>
    {
        public List<T> Value { get; set; } = [];
    }

    public sealed class RoleItemDto
    {
        public string Id { get; set; } = default!;
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = default!;
        public string? CreatedFullName { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedFullName { get; set; }
    }
}
