using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.Reservations;

public interface IReservationRepository : IAuditableRepository<Reservation>
{
}