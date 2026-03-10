using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;

namespace DeKayaServer.Application.CoolingRooms;

public static class CoolingRoomMappingExtensions
{
    public static IQueryable<CoolingRoomDto> MapTo(
        this IQueryable<EntityWithAuditDto<CoolingRoom>> entities,
        IQueryable<CoolingRoomStatus> roomStatuses )
    {
        var res = entities
            .Join( roomStatuses, y => y.Entity.RoomStatusId, y => y.Id, ( e, status )
                => new { e.Entity, e.CreatedUser, e.UpdatedUser, Status = status } )
            .Select( x => new CoolingRoomDto
            {
                Id = x.Entity.Id,
                RoomName = x.Entity.RoomName.Value,
                DailyPrice = x.Entity.DailyPrice.Value,
                Shelf = x.Entity.Shelf.Value,
                IsActive = x.Entity.IsActive,
                StatusId = x.Status.Id,
                StatusName = x.Status.StatusName.Value,
                CreatedAt = x.Entity.CreatedAt,
                CreatedBy = x.Entity.CreatedBy.Value,
                CreatedFullName = x.CreatedUser.FullName.Value,
                UpdatedAt = x.Entity.UpdatedAt,
                UpdatedBy = x.Entity.UpdatedBy != null ? x.Entity.UpdatedBy.Value : null,
                UpdatedFullName = x.UpdatedUser != null ? x.UpdatedUser.FullName.Value : null,
            } );
        return res;
    }
}
