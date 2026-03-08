using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class CoolingRoomRepository( ApplicationDbContext context ) : AuditableRepository<CoolingRoom, ApplicationDbContext>( context ), ICoolingRoomRepository
{
}
