using DeKayaServer.Contracts.Reservations;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Reservations;

namespace DeKayaServer.Application.Reservations;

public static class ReservationMappingExtensions
{
    public static IQueryable<ReservationDto> MapTo(
        this IQueryable<EntityWithAuditDto<Reservation>> entities )
    {
        var res = entities
            .Select( x => new ReservationDto
            {
                Id = x.Entity.Id,
                IsActive = x.Entity.IsActive,

                CustomerId = x.Entity.CustomerId,
                DeliveryLocation = x.Entity.DeliveryLocation.Value,
                DeliveryDate = x.Entity.DeliveryDate.Value,
                DeliveryTime = x.Entity.DeliveryDate.Value.ToDateTime( x.Entity.DeliveryTime.Value ),
                PickUpDate = x.Entity.PickUpDate.Value,
                PickUpTime = x.Entity.PickUpDate.Value.ToDateTime( x.Entity.PickUpTime.Value ),
                TotalDay = x.Entity.TotalDay.Value,
                CoolingRoomId = x.Entity.CoolingRoomId,
                CoolingRoomDailyPrice = x.Entity.CoolingRoomDailyPrice.Value,
                ReservationTotalAmount = x.Entity.ReservationTotalAmount.Value,
                PaidAtReservation = x.Entity.PaidAtReservation.Value,
                Note = x.Entity.Note != null ? x.Entity.Note.Value : null,

                CreatedAt = x.Entity.CreatedAt,
                CreatedBy = x.CreatedUser.CreatedBy,
                CreatedFullName = x.CreatedUser.FullName.Value,
                UpdatedAt = x.Entity.UpdatedAt,
                UpdatedBy = x.Entity.UpdatedBy != null ? x.Entity.UpdatedBy.Value : null,
                UpdatedFullName = x.UpdatedUser != null ? x.UpdatedUser.FullName.Value : null
            } );
        return res;
    }
}
