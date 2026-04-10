using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Reservations;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:delete" )]
public sealed record ReservationDeleteCommand( Guid Id ) : IRequest<Result<string>>;

internal sealed class ReservationDeleteCommandHandler(
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle( ReservationDeleteCommand request, CancellationToken cancellationToken )
    {
        var reservation = await reservationRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( reservation is null )
        {
            return Result<string>.Failure( "Rezervasyon bulunamadı" );
        }
        reservation.Delete();
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Rezervasyon başarıyla silindi";
    }
}