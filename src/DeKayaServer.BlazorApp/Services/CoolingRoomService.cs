using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.CoolingRooms;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface ICoolingRoomService
{
    Task<Result<string>> CreateAsync( CreateCoolingRoomRequest request, CancellationToken cancellationToken = default );
    Task<Result<string>> UpdateAsync( Guid Id, UpdateCoolingRoomRequest request, CancellationToken cancellationToken = default );
    Task<Result<CoolingRoomDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<List<CoolingRoomDto>>> GetAllAsync( CancellationToken cancellationToken = default );
    Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
}

public class CoolingRoomService( IApiClient apiClient ) : ICoolingRoomService
{
    public Task<Result<string>> CreateAsync( CreateCoolingRoomRequest request, CancellationToken cancellationToken = default )
        => apiClient.PostAsync<CreateCoolingRoomRequest, string>(
         EndpointConstants.CoolingRooms,
         request,
         cancellationToken );

    public Task<Result<string>> UpdateAsync( Guid Id, UpdateCoolingRoomRequest request, CancellationToken cancellationToken = default )
        => apiClient.PutAsync<UpdateCoolingRoomRequest, string>(
         EndpointConstants.CoolingRooms,
         request,
         cancellationToken );

    public Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.DeleteAsync<string>( $"{EndpointConstants.CoolingRooms}/{id}", cancellationToken );

    public Task<Result<CoolingRoomDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.GetAsync<CoolingRoomDto>( $"{EndpointConstants.CoolingRooms}/{id}", cancellationToken );

    public async Task<Result<List<CoolingRoomDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<CoolingRoomDto>>( EndpointConstants.ODataCoolingRooms, cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<CoolingRoomDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<CoolingRoomDto>>
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
