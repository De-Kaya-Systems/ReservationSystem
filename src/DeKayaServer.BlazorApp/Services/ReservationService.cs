using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.Reservations;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface IReservationService
{
    Task<Result<string>> CreateAsync( CreateReservationRequest request, CancellationToken cancellationToken = default );
    Task<Result<string>> UpdateAsync( Guid Id, UpdateReservationRequest request, CancellationToken cancellationToken = default );
    Task<Result<ReservationDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<List<ReservationDto>>> GetAllAsync( CancellationToken cancellationToken = default );
    Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
}

public class ReservationService( IApiClient apiClient ) : IReservationService
{
    public Task<Result<string>> CreateAsync( CreateReservationRequest request, CancellationToken cancellationToken = default )
        => apiClient.PostAsync<CreateReservationRequest, string>(
            EndpointConstants.Reservations,
            request,
            cancellationToken );
    public Task<Result<string>> UpdateAsync( Guid Id, UpdateReservationRequest request, CancellationToken cancellationToken = default )
        => apiClient.PutAsync<UpdateReservationRequest, string>(
            EndpointConstants.Reservations,
            request,
            cancellationToken );
    public Task<Result<ReservationDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.GetAsync<ReservationDto>( $"{EndpointConstants.Reservations}/{id}", cancellationToken );

    public Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.DeleteAsync<string>( $"{EndpointConstants.Reservations}/{id}", cancellationToken );

    public async Task<Result<List<ReservationDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<ReservationDto>>( EndpointConstants.ODataReservations, cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<ReservationDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<ReservationDto>>
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
