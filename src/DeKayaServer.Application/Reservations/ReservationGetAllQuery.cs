using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Reservations;
using DeKayaServer.Domain.Reservations;
using TS.MediatR;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:getall" )]
public sealed record ReservationGetAllQuery : IRequest<IQueryable<ReservationDto>>;

internal sealed class ReservationGetAllQueryHandler(
    IReservationRepository reservationRepository ) : IRequestHandler<ReservationGetAllQuery, IQueryable<ReservationDto>>
{
    public Task<IQueryable<ReservationDto>> Handle( ReservationGetAllQuery request, CancellationToken cancellationToken )
    {
        var query = reservationRepository
            .GetAllWithAudit()
            .MapTo()
            .AsQueryable();
        return Task.FromResult( query );
    }
}
