using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

public sealed class CoolingRoomMaintenanceGetAllQuery : IRequest<Result<List<CoolingRoomMaintenanceLogDto>>>;

internal sealed class CoolingRoomMaintenanceGetAllQueryHandler(
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository,
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository ) : IRequestHandler<CoolingRoomMaintenanceGetAllQuery, Result<List<CoolingRoomMaintenanceLogDto>>>
{
    public async Task<Result<List<CoolingRoomMaintenanceLogDto>>> Handle( CoolingRoomMaintenanceGetAllQuery request, CancellationToken cancellationToken )
    {
        var maintenancesWithAudit = coolingRoomMaintenanceRepository
            .GetAllWithAudit()
            .IgnoreQueryFilters();

        var rooms = coolingRoomRepository.GetAll().IgnoreQueryFilters();
        var statuses = coolingRoomStatusRepository.GetAll().IgnoreQueryFilters();

        var query =
            from m in maintenancesWithAudit
            join r in rooms on m.Entity.CoolingRoomId equals r.Id
            join s in statuses on r.RoomStatusId equals s.Id
            orderby m.Entity.CreatedAt descending
            select new CoolingRoomMaintenanceLogDto
            {
                MaintenanceId = ( Guid )m.Entity.Id,
                Description = m.Entity.Description.Value,
                MaintenanceDateStart = m.Entity.MaintenanceDateStart.Value,
                MaintenanceDateEnd = m.Entity.MaintenanceDateEnd.Value,

                IsDeleted = m.Entity.IsDeleted,
                DeletedAt = m.Entity.DeletedAt,
                DeletedBy = m.Entity.DeletedBy != null ? m.Entity.DeletedBy.Value : null,

                CoolingRoomId = r.Id,
                RoomName = r.RoomName.Value,
                StatusId = s.Id,
                StatusName = s.StatusName.Value,

                CreatedAt = m.Entity.CreatedAt,
                CreatedBy = m.Entity.CreatedBy,
                CreatedFullName = m.Entity.CreatedByFullName,
                UpdatedAt = m.Entity.UpdatedAt,
                UpdatedBy = m.Entity.UpdatedBy != null ? m.Entity.UpdatedBy.Value : null,
                UpdatedFullName = m.UpdatedUser != null ? m.UpdatedUser.FullName.Value : null,
            };

        var list = await query.ToListAsync( cancellationToken );
        return Result<List<CoolingRoomMaintenanceLogDto>>.Succeed( list );
    }
}