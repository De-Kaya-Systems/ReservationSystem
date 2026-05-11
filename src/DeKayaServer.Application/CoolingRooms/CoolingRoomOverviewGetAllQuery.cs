using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.CustomerBalance.Enum;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.Reservations;
using DeKayaServer.Domain.Reservations.Enum;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:getall" )]
public sealed record CoolingRoomOverviewGetAllQuery : IRequest<Result<List<CoolingRoomOverviewDto>>>;

internal sealed class CoolingRoomOverviewGetAllQueryHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    IReservationRepository reservationRepository,
    ICustomerRepository customerRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository )
    : IRequestHandler<CoolingRoomOverviewGetAllQuery, Result<List<CoolingRoomOverviewDto>>>
{
    public async Task<Result<List<CoolingRoomOverviewDto>>> Handle(
        CoolingRoomOverviewGetAllQuery request,
        CancellationToken cancellationToken )
    {
        var now = DateTime.Now;

        var rooms = await coolingRoomRepository
            .GetAllWithAudit()
            .Select( x => new
            {
                x.Entity.Id,
                RoomName = x.Entity.RoomName.Value,
                DailyPrice = x.Entity.DailyPrice.Value,
                Shelf = x.Entity.Shelf.Value,
                RoomStatusId = x.Entity.RoomStatusId.Value,
                MaintenanceId = x.Entity.MaintenanceId != null
                    ? x.Entity.MaintenanceId.Value
                    : ( Guid? )null,
                x.Entity.IsActive
            } )
            .ToListAsync( cancellationToken );

        var statuses = await coolingRoomStatusRepository
            .GetAll()
            .Select( x => new
            {
                x.Id,
                StatusName = x.StatusName.Value
            } )
            .ToListAsync( cancellationToken );

        var reservations = await reservationRepository
            .GetAll()
            .Where( x => x.Status != ReservationStatus.Completed
                        && x.Status != ReservationStatus.Cancelled )
            .Select( x => new ReservationProjection
            {
                Id = x.Id,
                CoolingRoomId = x.CoolingRoomId.Value,
                CustomerId = x.CustomerId.Value,
                ReservationNumber = x.ReservationNumber.Value,
                ReservationStart = x.DeliveryDate.Value.ToDateTime( x.DeliveryTime.Value ),
                ReservationEnd = x.PickUpDate.Value.ToDateTime( x.PickUpTime.Value ),
                Status = x.Status,
                DeliveredAt = x.DeliveredAt != null ? x.DeliveredAt.Value : null,
                PickedUpAt = x.PickedUpAt != null ? x.PickedUpAt.Value : null
            } )
            .ToListAsync( cancellationToken );

        var customerIds = reservations
            .Select( x => x.CustomerId )
            .Distinct()
            .ToList();

        var customers = await customerRepository
            .GetAll()
            .Where( x => customerIds.Contains( x.Id ) )
            .Select( x => new
            {
                x.Id,
                FullName = x.FullName.Value,
                x.Contact.PhoneNumber
            } )
            .ToListAsync( cancellationToken );

        var balances = await customerBalanceRepository
            .GetAll()
            .Where( x => x.SourceType == BalanceSourceType.Reservation )
            .Select( x => new
            {
                SourceId = x.SourceId != null ? x.SourceId.Value : ( Guid? )null,
                OutstandingAmount = x.OutstandingAmount.Value
            } )
            .ToListAsync( cancellationToken );

        var maintenances = await coolingRoomMaintenanceRepository
            .GetAll()
            .Select( x => new
            {
                x.Id,
                CoolingRoomId = x.CoolingRoomId.Value,
                Description = x.Description.Value,
                MaintenanceEnd = x.MaintenanceDateEnd.Value
            } )
            .ToListAsync( cancellationToken );

        var result = rooms
            .Select( room =>
            {
                var status = statuses.FirstOrDefault( x => x.Id == room.RoomStatusId );

                var roomReservations = reservations
                    .Where( x => x.CoolingRoomId == room.Id )
                    .OrderBy( x => x.ReservationStart )
                    .ToList();

                var currentReservation = roomReservations
                    .Where( x => IsCurrentReservation( x, now ) )
                    .OrderBy( x => x.ReservationStart )
                    .FirstOrDefault();

                var nextReservation = roomReservations
                    .Where( x => x.ReservationStart > now )
                    .OrderBy( x => x.ReservationStart )
                    .FirstOrDefault();

                var currentCustomer = currentReservation is not null
                    ? customers.FirstOrDefault( x => x.Id == currentReservation.CustomerId )
                    : null;

                var nextCustomer = nextReservation is not null
                    ? customers.FirstOrDefault( x => x.Id == nextReservation.CustomerId )
                    : null;

                var currentOutstandingAmount = currentReservation is not null
                    ? balances
                        .Where( x => x.SourceId == currentReservation.Id )
                        .Sum( x => x.OutstandingAmount )
                    : 0;

                var maintenance = room.MaintenanceId.HasValue
                    ? maintenances.FirstOrDefault( x => x.Id == room.MaintenanceId.Value )
                    : null;

                return new CoolingRoomOverviewDto
                {
                    Id = room.Id,
                    RoomName = room.RoomName,
                    DailyPrice = room.DailyPrice,
                    Shelf = room.Shelf,
                    IsActive = room.IsActive,

                    StatusId = room.RoomStatusId,
                    StatusName = status?.StatusName,

                    CurrentReservationId = currentReservation?.Id,
                    CurrentReservationNumber = currentReservation?.ReservationNumber,
                    CurrentCustomerName = currentCustomer?.FullName,
                    CurrentCustomerPhoneNumber = currentCustomer?.PhoneNumber,
                    CurrentReservationStart = currentReservation?.ReservationStart,
                    CurrentReservationEnd = currentReservation?.ReservationEnd,
                    CurrentReservationStatus = currentReservation is not null ? ( int )currentReservation.Status : null,
                    DeliveredAt = currentReservation?.DeliveredAt,
                    PickedUpAt = currentReservation?.PickedUpAt,

                    NextReservationId = nextReservation?.Id,
                    NextReservationNumber = nextReservation?.ReservationNumber,
                    NextCustomerName = nextCustomer?.FullName,
                    NextReservationStart = nextReservation?.ReservationStart,
                    NextReservationEnd = nextReservation?.ReservationEnd,

                    NextAvailableAt = GetNextAvailableAt( currentReservation ),

                    OutstandingAmount = currentOutstandingAmount,

                    MaintenanceId = room.MaintenanceId,
                    MaintenanceDescription = maintenance?.Description,
                    MaintenanceEnd = maintenance?.MaintenanceEnd.ToDateTime( TimeOnly.MinValue )
                };
            } )
            .OrderBy( x => x.RoomName )
            .ToList();

        return Result<List<CoolingRoomOverviewDto>>.Succeed( result );
    }

    private static bool IsCurrentReservation( ReservationProjection reservation, DateTime now )
    {
        if ( reservation.Status == ReservationStatus.DeliveredToCustomer )
        {
            return true;
        }

        if ( reservation.Status == ReservationStatus.Scheduled
             && reservation.ReservationStart <= now )
        {
            return true;
        }

        return false;
    }

    private static DateTime? GetNextAvailableAt( ReservationProjection? currentReservation )
    {
        if ( currentReservation is not null )
        {
            return currentReservation.ReservationEnd;
        }

        return null;
    }

    private sealed class ReservationProjection
    {
        public Guid Id { get; set; }
        public Guid CoolingRoomId { get; set; }
        public Guid CustomerId { get; set; }
        public string? ReservationNumber { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
    }
}