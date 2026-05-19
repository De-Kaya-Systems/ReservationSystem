using DeKayaServer.Domain.Constants;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.Reservations;
using DeKayaServer.Domain.Reservations.Enum;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms.Services;

internal class CoolingRoomAvailabilityService(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository,
    IReservationRepository reservationRepository ) : ICoolingRoomAvailabilityService
{
    public async Task<Result<string>> EnsureCanReserveAsync(
        Guid coolingRoomId,
        DateOnly deliveryDate,
        DateOnly pickUpDate,
        Guid? excludedReservationId,
        CancellationToken cancellationToken )
    {
        if ( pickUpDate < deliveryDate )
        {
            return Result<string>.Failure( "Teslim alma tarihi teslim tarihinden önce olamaz." );
        }

        var coolingRoom = await coolingRoomRepository.FirstOrDefaultAsync(
            x => x.Id == coolingRoomId,
            cancellationToken );

        if ( coolingRoom is null )
        {
            return Result<string>.Failure( "Soğuk oda bulunamadı." );
        }

        if ( !coolingRoom.IsActive )
        {
            return Result<string>.Failure( "Seçilen soğuk oda aktif değil." );
        }

        var roomStatus = await coolingRoomStatusRepository.FirstOrDefaultAsync(
            x => x.Id == coolingRoom.RoomStatusId,
            cancellationToken );

        if ( roomStatus is null )
        {
            return Result<string>.Failure( "Soğuk oda durumu bulunamadı." );
        }

        if ( IsFaultyStatus( roomStatus.StatusName.Value ) )
        {
            return Result<string>.Failure( "Seçilen soğuk oda arıza/bakımda." );
        }

        var hasMaintenanceOverlap = await coolingRoomMaintenanceRepository.AnyAsync(
            x => x.CoolingRoomId == coolingRoomId
                && x.MaintenanceDateStart.Value <= pickUpDate
                && deliveryDate <= x.MaintenanceDateEnd.Value,
            cancellationToken );

        if ( hasMaintenanceOverlap )
        {
            return Result<string>.Failure( "Seçilen soğuk oda bu tarihler arasında bakımda." );
        }

        var hasReservationOverlap = await reservationRepository.AnyAsync(
            x => x.CoolingRoomId == coolingRoomId
                && x.Status != ReservationStatus.Completed
                && x.Status != ReservationStatus.Cancelled
                && x.DeliveryDate.Value <= pickUpDate
                && deliveryDate <= x.PickUpDate.Value
                && ( excludedReservationId == null || x.Id != excludedReservationId.Value ),
            cancellationToken );

        if ( hasReservationOverlap )
        {
            return Result<string>.Failure( "Seçilen soğuk oda bu tarihler arasında rezerve edilmiş." );
        }

        return Result<string>.Succeed( string.Empty );
    }

    private static bool IsFaultyStatus( string? statusName )
    => string.Equals(
        statusName?.Trim(),
        CoolingRoomStatusConstants.Faulty,
        StringComparison.OrdinalIgnoreCase );
}
