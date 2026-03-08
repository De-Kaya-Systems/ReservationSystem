using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.CoolingRoomStatus;

public interface ICoolingRoomStatusRepository : IAuditableRepository<CoolingRoomStatus>
{
}