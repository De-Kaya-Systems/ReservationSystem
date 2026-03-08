using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class CoolingRoomStatusRepository( ApplicationDbContext context ) : AuditableRepository<CoolingRoomStatus, ApplicationDbContext>( context ), ICoolingRoomStatusRepository
{
}