using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Constants;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.Reservations;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:delete" )]
public sealed record ReservationDeleteCommand( Guid Id ) : IRequest<Result<string>>;

internal sealed class ReservationDeleteCommandHandler(
    IReservationRepository reservationRepository,
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle( ReservationDeleteCommand request, CancellationToken cancellationToken )
    {
        var reservation = await reservationRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( reservation is null )
        {
            return Result<string>.Failure( "Rezervasyon bulunamadı" );
        }

        // Reservation sil
        // EN : Delete the reservation
        reservation.Delete();

        // CoolingRoom'u bul
        // EN : Find the CoolingRoom
        var coolingRoom = await coolingRoomRepository.FirstOrDefaultAsync(
            x => x.Id == reservation.CoolingRoomId,
            cancellationToken );

        if ( coolingRoom is not null )
        {
            // "Uygun" status'unu bul
            // EN : Find the "Available" status
            var availableStatus = await coolingRoomStatusRepository.FirstOrDefaultAsync(
                x => x.StatusName.Value == CoolingRoomStatusConstants.Available,
                cancellationToken );

            if ( availableStatus is not null )
            {
                // CoolingRoom'un status'unu "Uygun" olarak güncelle
                // EN : Update the CoolingRoom's status to "Available"
                coolingRoom.SetRoomStatusId( availableStatus.Id );
                coolingRoomRepository.Update( coolingRoom );
            }
        }

        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Rezervasyon başarıyla silindi";
    }
}