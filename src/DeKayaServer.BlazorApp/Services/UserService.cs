using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.Contracts.Users;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public class UserService( IApiClient apiClient ) : IUserService
{
    public Task<Result<string>> CreateAsync(
       string firstName,
       string lastName,
       string email,
       string userName,
       Guid roleId,
       CancellationToken cancellationToken = default )
       => apiClient.PostAsync<CreateUserRequest, string>(
           EndpointConstants.Users,
           new CreateUserRequest( firstName, lastName, email, userName, roleId ),
           cancellationToken );

    public Task<Result<string>> UpdateAsync(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string userName,
        Guid roleId,
        CancellationToken cancellationToken = default )
        => apiClient.PutAsync<UpdateUserRequest, string>(
            EndpointConstants.Users,
            new UpdateUserRequest( id, firstName, lastName, email, userName, roleId ),
            cancellationToken );

    public Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.DeleteAsync<string>( $"{EndpointConstants.Users}/{id}", cancellationToken );

    public Task<Result<UserDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.GetAsync<UserDto>( $"{EndpointConstants.Users}/{id}", cancellationToken );

    public async Task<Result<List<UserDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<UserDto>>( EndpointConstants.ODataUsers, cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<UserDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<UserDto>>
        {
            IsSuccessful = true,
            StatusCode = odataRes.StatusCode,
            Data = odataRes.Data.Value ?? []
        };
    }
    private sealed class ODataEnvelope<T>
    {
        public List<T> Value { get; set; } = [];
    }
}
