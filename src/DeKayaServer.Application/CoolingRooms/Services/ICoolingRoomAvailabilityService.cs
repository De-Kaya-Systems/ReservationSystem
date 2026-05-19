using TS.Result;

namespace DeKayaServer.Application.CoolingRooms.Services;

internal interface ICoolingRoomAvailabilityService
{
    Task<Result<string>> EnsureCanReserveAsync(
        Guid coolingRoomId,
        DateOnly deliveryDate,
        DateOnly pickUpDate,
        Guid? excludedReservationId,
        CancellationToken cancellationToken );
}
