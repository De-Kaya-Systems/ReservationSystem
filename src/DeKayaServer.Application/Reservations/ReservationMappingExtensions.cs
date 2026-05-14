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
                ReservationNumber = x.Entity.ReservationNumber.Value,
                DeliveryLocation = x.Entity.DeliveryLocation.Value,
                DeliveryDate = x.Entity.DeliveryDate.Value,
                DeliveryTime = x.Entity.DeliveryDate.Value.ToDateTime( x.Entity.DeliveryTime.Value ),
                PickUpDate = x.Entity.PickUpDate.Value,
                PickUpTime = x.Entity.PickUpDate.Value.ToDateTime( x.Entity.PickUpTime.Value ),
                Status = ( int )x.Entity.Status,
                DeliveredAt = x.Entity.DeliveredAt != null ? x.Entity.DeliveredAt.Value : null,
                PickedUpAt = x.Entity.PickedUpAt != null ? x.Entity.PickedUpAt.Value : null,
                TotalDay = x.Entity.TotalDay.Value,
                CoolingRoomId = x.Entity.CoolingRoomId,
                CoolingRoomBaseDailyPrice = x.Entity.CoolingRoomBaseDailyPrice.Value,
                CoolingRoomDailyPrice = x.Entity.CoolingRoomDailyPrice.Value,
                HasPriceOverride = x.Entity.CoolingRoomDailyPrice.Value != x.Entity.CoolingRoomBaseDailyPrice.Value,
                PriceOverrideReason = x.Entity.PriceOverrideReason != null ? x.Entity.PriceOverrideReason.Value : null,
                PriceOverrideNote = x.Entity.PriceOverrideNote != null ? x.Entity.PriceOverrideNote.Value : null,
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
