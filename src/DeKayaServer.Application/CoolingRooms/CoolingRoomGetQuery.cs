using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:get" )]
public sealed record CoolingRoomGetQuery( Guid Id ) : IRequest<Result<CoolingRoomDto>>;

internal sealed class CoolingRoomGetQueryHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository ) : IRequestHandler<CoolingRoomGetQuery, Result<CoolingRoomDto>>
{
    public async Task<Result<CoolingRoomDto>> Handle( CoolingRoomGetQuery request, CancellationToken cancellationToken )
    {
        var statuses = coolingRoomStatusRepository.GetAll();
        var maintenances = coolingRoomMaintenanceRepository.GetAll();

        var query =
            from r in coolingRoomRepository.GetAllWithAudit()
            join s in statuses on r.Entity.RoomStatusId equals s.Id
            join m in maintenances on r.Entity.MaintenanceId equals m.Id into mm
            from m in mm.DefaultIfEmpty()
            where r.Entity.Id == request.Id
            select new CoolingRoomDto
            {
                Id = r.Entity.Id,
                RoomName = r.Entity.RoomName.Value,
                DailyPrice = r.Entity.DailyPrice.Value,
                Shelf = r.Entity.Shelf.Value,
                IsActive = r.Entity.IsActive,

                StatusId = s.Id,
                StatusName = s.StatusName.Value,

                MaintenanceId = r.Entity.MaintenanceId!,
                Maintenance = m == null ? null : new CoolingRoomMaintenanceDto
                {
                    Id = m.Id,
                    Description = m.Description.Value,
                    MaintenanceDateStart = m.MaintenanceDateStart.Value,
                    MaintenanceDateEnd = m.MaintenanceDateEnd.Value,
                    StatusId = m.StatusId
                },

                CreatedAt = r.Entity.CreatedAt,
                CreatedBy = r.Entity.CreatedBy.Value,
                CreatedFullName = r.CreatedUser.FullName.Value,
                UpdatedAt = r.Entity.UpdatedAt,
                UpdatedBy = r.Entity.UpdatedBy != null ? r.Entity.UpdatedBy!.Value : null,
                UpdatedFullName = r.UpdatedUser != null ? r.UpdatedUser.FullName.Value : null,
            };

        var result = await query.FirstOrDefaultAsync( cancellationToken );

        if ( result is null )
            return Result<CoolingRoomDto>.Failure( "Oda bulunamadı...." );

        return result;
    }
}