using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.CoolingRooms;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:delete" )]
public sealed record CoolingRoomDeleteCommand( Guid Id ) : IRequest<Result<string>>;

internal sealed class CoolingRoomDeleteCommandHandler(
    ICoolingRoomRepository coolingRoomRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<CoolingRoomDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle( CoolingRoomDeleteCommand request, CancellationToken cancellationToken )
    {
        var coolingRoom = await coolingRoomRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( coolingRoom is null )
        {
            return Result<string>.Failure( "Oda bulunamadı" );
        }

        coolingRoom.Delete();
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Oda silindi";
    }
}