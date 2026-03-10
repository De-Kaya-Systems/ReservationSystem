using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:get" )]
public sealed record CoolingRoomGetQuery( Guid Id ) : IRequest<Result<CoolingRoomDto>>;

internal sealed class CoolingRoomGetQueryHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository ) : IRequestHandler<CoolingRoomGetQuery, Result<CoolingRoomDto>>
{
    public async Task<Result<CoolingRoomDto>> Handle( CoolingRoomGetQuery request, CancellationToken cancellationToken )
    {
        var result = await coolingRoomRepository
            .GetAllWithAudit()
            .MapTo( coolingRoomStatusRepository.GetAll() )
            .Where( x => x.Id == request.Id )
            .FirstOrDefaultAsync( cancellationToken );

        if ( result is null )
        {
            return Result<CoolingRoomDto>.Failure( "Oda bulunamadı...." );
        }
        return result;
    }
}