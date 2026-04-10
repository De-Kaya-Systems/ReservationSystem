using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Reservations;
using DeKayaServer.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:get" )]
public sealed record ReservationGetQuery( Guid Id ) : IRequest<Result<ReservationDto>>;

internal sealed class ReservationGetQueryHandler(
    IReservationRepository reservationRepository ) : IRequestHandler<ReservationGetQuery, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle( ReservationGetQuery request, CancellationToken cancellationToken )
    {
        var res = await reservationRepository
             .GetAllWithAudit()
             .MapTo()
             .Where( x => x.Id == request.Id )
             .FirstOrDefaultAsync( cancellationToken );
        if ( res is null )
        {
            return Result<ReservationDto>.Failure( "Reservation not found" );
        }
        return res;
    }
}
