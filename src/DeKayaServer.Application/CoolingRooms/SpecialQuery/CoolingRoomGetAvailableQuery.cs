using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.Constants;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms.SpecialQuery;

public sealed record CoolingRoomGetAvailableQuery( DateOnly From, DateOnly To ) : IRequest<Result<List<CoolingRoomDto>>>;

internal sealed class CoolingRoomGetAvailableQueryHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository,
    IReservationRepository reservationRepository ) : IRequestHandler<CoolingRoomGetAvailableQuery, Result<List<CoolingRoomDto>>>
{
    public async Task<Result<List<CoolingRoomDto>>> Handle( CoolingRoomGetAvailableQuery request, CancellationToken cancellationToken )
    {
        if ( request.To < request.From )
        {
            return Result<List<CoolingRoomDto>>.Failure( "Teslim alma tarihi teslim tarihinden önce olamaz." );
        }

        var statuses = coolingRoomStatusRepository.GetAll();
        var maintenances = coolingRoomMaintenanceRepository.GetAll();
        var reservations = reservationRepository.GetAll();

        var query =
          from r in coolingRoomRepository.GetAllWithAudit()
          join s in statuses on r.Entity.RoomStatusId equals s.Id
          join m in maintenances on r.Entity.MaintenanceId equals m.Id into mm
          from m in mm.DefaultIfEmpty()
          where r.Entity.IsActive
          where s.StatusName.Value.Trim().ToLower() != CoolingRoomStatusConstants.Faulty.ToLower()
          where m == null || !( m.MaintenanceDateStart.Value <= request.To && request.From <= m.MaintenanceDateEnd.Value )
          where !reservations.Any( res =>
              res.CoolingRoomId == r.Entity.Id
              && res.DeliveryDate.Value <= request.To
              && request.From <= res.PickUpDate.Value )
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
              UpdatedBy = r.Entity.UpdatedBy == null ? null : r.Entity.UpdatedBy!.Value,
              UpdatedFullName = r.UpdatedUser != null ? r.UpdatedUser.FullName.Value : null
          };

        var data = await query.ToListAsync( cancellationToken );
        return data;
    }
}