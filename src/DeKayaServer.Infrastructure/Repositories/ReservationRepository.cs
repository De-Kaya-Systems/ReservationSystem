using DeKayaServer.Domain.Reservations;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class ReservationRepository(ApplicationDbContext context) : AuditableRepository<Reservation, ApplicationDbContext>( context ), IReservationRepository
{
}