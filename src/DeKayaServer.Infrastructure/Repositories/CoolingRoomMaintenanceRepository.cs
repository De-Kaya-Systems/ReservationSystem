using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class CoolingRoomMaintenanceRepository( ApplicationDbContext context ) : AuditableRepository<CoolingRoomMaintenance, ApplicationDbContext>( context ), ICoolingRoomMaintenanceRepository
{
}