using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.CoolingRooms;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface ICoolingRoomMaintenanceService
{
    Task<Result<List<CoolingRoomMaintenanceLogDto>>> GetAllAsync( CancellationToken cancellationToken = default );
}

public class CoolingRoomMaintenanceService( IApiClient apiClient ) : ICoolingRoomMaintenanceService
{
    public Task<Result<List<CoolingRoomMaintenanceLogDto>>> GetAllAsync( CancellationToken cancellationToken = default )
        => apiClient.GetAsync<List<CoolingRoomMaintenanceLogDto>>( EndpointConstants.CoolingRoomMaintenances, cancellationToken );
}
