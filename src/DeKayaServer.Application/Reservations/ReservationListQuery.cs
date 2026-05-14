using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Common;
using DeKayaServer.Contracts.Reservations;
using DeKayaServer.Contracts.Reservations.Enum;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:getall" )]
public sealed record ReservationListQuery(
    string? CustomerName,
    DateOnly? ReservationStartDate,
    DateOnly? ReservationEndDate,
    ReservationOperationFilterDto? OperationStatus,
    int PageIndex,
    int PageSize )
    : IRequest<Result<PagedResultDto<ReservationListItemDto>>>;

internal sealed class ReservationListQueryHandler(
    IReservationRepository reservationRepository,
    ICustomerRepository customerRepository,
    ICoolingRoomRepository coolingRoomRepository )
    : IRequestHandler<ReservationListQuery, Result<PagedResultDto<ReservationListItemDto>>>
{
    public async Task<Result<PagedResultDto<ReservationListItemDto>>> Handle(
        ReservationListQuery request,
        CancellationToken cancellationToken )
    {
        var pageIndex = Math.Max( request.PageIndex, 0 );
        var pageSize = Math.Clamp( request.PageSize <= 0 ? 25 : request.PageSize, 1, 100 );
        var operationWindowEnd = DateTime.Now.AddHours( 24 );

        var reservationQuery = reservationRepository.GetAllWithAudit();

        if ( !string.IsNullOrWhiteSpace( request.CustomerName ) )
        {
            var customerName = request.CustomerName.Trim();

            var matchingCustomerIds = await customerRepository
                .GetAll()
                .Where( x => EF.Functions.Like( x.FullName.Value, $"%{customerName}%" ) )
                .Select( x => ( Guid )x.Id )
                .ToListAsync( cancellationToken );

            if ( matchingCustomerIds.Count == 0 )
            {
                return EmptyPage( pageIndex, pageSize );
            }

            reservationQuery = reservationQuery
                .Where( x => matchingCustomerIds.Contains( ( Guid )x.Entity.CustomerId ) );
        }

        var query = reservationQuery.MapTo();

        if ( request.ReservationStartDate is not null )
        {
            query = query.Where( x => x.DeliveryDate == request.ReservationStartDate.Value );
        }

        if ( request.ReservationEndDate is not null )
        {
            query = query.Where( x => x.PickUpDate == request.ReservationEndDate.Value );
        }

        query = ApplyOperationStatusFilter(
            query,
            request.OperationStatus,
            operationWindowEnd );

        var totalCount = await query.CountAsync( cancellationToken );

        var rows = await query
            .OrderBy( x => x.DeliveryTime )
            .ThenBy( x => x.ReservationNumber )
            .Skip( pageIndex * pageSize )
            .Take( pageSize )
            .ToListAsync( cancellationToken );

        var customerIdsOnPage = rows
            .Select( x => TryParseGuid( x.CustomerId ) )
            .Where( x => x.HasValue )
            .Select( x => x!.Value )
            .Distinct()
            .ToList();

        var roomIdsOnPage = rows
            .Select( x => TryParseGuid( x.CoolingRoomId ) )
            .Where( x => x.HasValue )
            .Select( x => x!.Value )
            .Distinct()
            .ToList();

        var customerRows = await customerRepository
            .GetAll()
            .Where( x => customerIdsOnPage.Contains( ( Guid )x.Id ) )
            .Select( x => new
            {
                x.Id,
                FullName = x.FullName.Value
            } )
            .ToListAsync( cancellationToken );

        var customers = customerRows
            .ToDictionary( x => ( Guid )x.Id, x => x.FullName );

        var roomRows = await coolingRoomRepository
            .GetAll()
            .Where( x => roomIdsOnPage.Contains( ( Guid )x.Id ) )
            .Select( x => new
            {
                x.Id,
                RoomName = x.RoomName.Value
            } )
            .ToListAsync( cancellationToken );

        var rooms = roomRows
            .ToDictionary( x => ( Guid )x.Id, x => x.RoomName );

        var items = rows
            .Select( x =>
            {
                var customerId = TryParseGuid( x.CustomerId ) ?? Guid.Empty;
                var coolingRoomId = TryParseGuid( x.CoolingRoomId ) ?? Guid.Empty;
                var operationStatus = GetOperationStatus( x, operationWindowEnd );

                return new ReservationListItemDto
                {
                    Id = x.Id,
                    ReservationNumber = x.ReservationNumber,

                    CustomerId = customerId,
                    CustomerFullName = customers.TryGetValue( customerId, out var customerName )
                        ? customerName
                        : x.CustomerId,

                    CoolingRoomId = coolingRoomId,
                    CoolingRoomName = rooms.TryGetValue( coolingRoomId, out var roomName )
                        ? roomName
                        : x.CoolingRoomId,

                    ReservationStart = x.DeliveryTime,
                    ReservationEnd = x.PickUpTime,

                    Status = x.Status,
                    OperationStatus = operationStatus,
                    OperationStatusText = GetOperationStatusText( operationStatus ),

                    DeliveredAt = x.DeliveredAt,
                    PickedUpAt = x.PickedUpAt,

                    TotalDays = x.TotalDay,
                    BaseDailyPrice = x.CoolingRoomBaseDailyPrice,
                    AppliedDailyPrice = x.CoolingRoomDailyPrice,
                    HasPriceOverride = x.HasPriceOverride,
                    PriceOverrideReason = x.PriceOverrideReason,
                    TotalAmount = x.ReservationTotalAmount ?? 0
                };
            } )
            .ToList();

        return Result<PagedResultDto<ReservationListItemDto>>.Succeed( new()
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = totalCount == 0
                ? 0
                : ( int )Math.Ceiling( totalCount / ( double )pageSize )
        } );
    }

    private static IQueryable<ReservationDto> ApplyOperationStatusFilter(
        IQueryable<ReservationDto> query,
        ReservationOperationFilterDto? operationStatus,
        DateTime operationWindowEnd )
    {
        if ( operationStatus is null )
        {
            return query;
        }

        return operationStatus.Value switch
        {
            ReservationOperationFilterDto.Reserved =>
                query.Where( x =>
                    x.Status == ( int )ReservationOperationStatusDto.Scheduled
                    && x.DeliveryTime > operationWindowEnd ),

            ReservationOperationFilterDto.WaitingDelivery =>
                query.Where( x =>
                    x.Status == ( int )ReservationOperationStatusDto.Scheduled
                    && x.DeliveryTime <= operationWindowEnd ),

            ReservationOperationFilterDto.DeliveredToCustomer =>
                query.Where( x =>
                    x.Status == ( int )ReservationOperationStatusDto.DeliveredToCustomer
                    && x.PickUpTime > operationWindowEnd ),

            ReservationOperationFilterDto.WaitingPickup =>
                query.Where( x =>
                    x.Status == ( int )ReservationOperationStatusDto.DeliveredToCustomer
                    && x.PickUpTime <= operationWindowEnd ),

            ReservationOperationFilterDto.PickedUpFromCustomer =>
                query.Where( x => x.Status == ( int )ReservationOperationStatusDto.PickedUpFromCustomer ),

            ReservationOperationFilterDto.Completed =>
                query.Where( x => x.Status == ( int )ReservationOperationStatusDto.Completed ),

            _ => query
        };
    }

    private static ReservationOperationFilterDto? GetOperationStatus(
        ReservationDto reservation,
        DateTime operationWindowEnd )
        => ( ReservationOperationStatusDto )reservation.Status switch
        {
            ReservationOperationStatusDto.Scheduled when reservation.DeliveryTime <= operationWindowEnd
                => ReservationOperationFilterDto.WaitingDelivery,

            ReservationOperationStatusDto.Scheduled
                => ReservationOperationFilterDto.Reserved,

            ReservationOperationStatusDto.DeliveredToCustomer when reservation.PickUpTime <= operationWindowEnd
                => ReservationOperationFilterDto.WaitingPickup,

            ReservationOperationStatusDto.DeliveredToCustomer
                => ReservationOperationFilterDto.DeliveredToCustomer,

            ReservationOperationStatusDto.PickedUpFromCustomer
                => ReservationOperationFilterDto.PickedUpFromCustomer,

            ReservationOperationStatusDto.Completed
                => ReservationOperationFilterDto.Completed,

            _ => null
        };

    private static string GetOperationStatusText( ReservationOperationFilterDto? status )
        => status switch
        {
            ReservationOperationFilterDto.Reserved => "Rezerve edildi",
            ReservationOperationFilterDto.WaitingDelivery => "Teslimat bekliyor",
            ReservationOperationFilterDto.DeliveredToCustomer => "Müşteriye teslim edildi",
            ReservationOperationFilterDto.WaitingPickup => "Geri alım bekleniyor",
            ReservationOperationFilterDto.PickedUpFromCustomer => "Müşteriden geri alındı",
            ReservationOperationFilterDto.Completed => "Tamamlandı",
            _ => "Bilinmiyor"
        };

    private static Result<PagedResultDto<ReservationListItemDto>> EmptyPage(
        int pageIndex,
        int pageSize )
        => Result<PagedResultDto<ReservationListItemDto>>.Succeed( new()
        {
            Items = [],
            TotalCount = 0,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = 0
        } );

    private static Guid? TryParseGuid( string? value )
        => Guid.TryParse( value, out var id ) ? id : null;
}
