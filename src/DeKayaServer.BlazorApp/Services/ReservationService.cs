using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.Common;
using DeKayaServer.Contracts.Reservations;
using DeKayaServer.Contracts.Reservations.Enum;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface IReservationService
{
    Task<Result<string>> CreateAsync( CreateReservationRequest request, CancellationToken cancellationToken = default );
    Task<Result<string>> UpdateAsync( Guid Id, UpdateReservationRequest request, CancellationToken cancellationToken = default );
    Task<Result<string>> CompleteAsync( Guid id, CompleteReservationRequest request, CancellationToken cancellationToken = default );
    Task<Result<ReservationDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<List<ReservationDto>>> GetAllAsync( CancellationToken cancellationToken = default );
    Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<PagedResultDto<ReservationListItemDto>>> GetListAsync(
        string? customerName,
        DateOnly? reservationStartDate,
        DateOnly? reservationEndDate,
        ReservationOperationFilterDto? operationStatus,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default );
}

public class ReservationService( IApiClient apiClient ) : IReservationService
{

    public Task<Result<PagedResultDto<ReservationListItemDto>>> GetListAsync(
        string? customerName,
        DateOnly? reservationStartDate,
        DateOnly? reservationEndDate,
        ReservationOperationFilterDto? operationStatus,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default )
    {
        var query = new List<string>
        {
            $"pageIndex={pageIndex}",
            $"pageSize={pageSize}"
        };

        if ( !string.IsNullOrWhiteSpace( customerName ) )
            query.Add( $"customerName={Uri.EscapeDataString( customerName.Trim() )}" );

        if ( reservationStartDate is not null )
            query.Add( $"reservationStartDate={reservationStartDate.Value:yyyy-MM-dd}" );

        if ( reservationEndDate is not null )
            query.Add( $"reservationEndDate={reservationEndDate.Value:yyyy-MM-dd}" );

        if ( operationStatus is not null )
            query.Add( $"operationStatus={( int )operationStatus.Value}" );

        var url = $"{EndpointConstants.Reservations}/list?{string.Join( "&", query )}";

        return apiClient.GetAsync<PagedResultDto<ReservationListItemDto>>( url, cancellationToken );
    }
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

    public Task<Result<string>> CompleteAsync( Guid id, CompleteReservationRequest request, CancellationToken cancellationToken = default )
    => apiClient.PutAsync<CompleteReservationRequest, string>(
        $"{EndpointConstants.Reservations}/{id}/complete",
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
