using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CoolingRooms;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using TS.MediatR;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:getall" )]
public sealed class CoolingRoomGetAllQuery : IRequest<IQueryable<CoolingRoomDto>>;

internal sealed class CoolingRoomGetAllQueryHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository ) : IRequestHandler<CoolingRoomGetAllQuery, IQueryable<CoolingRoomDto>>
{
    public Task<IQueryable<CoolingRoomDto>> Handle( CoolingRoomGetAllQuery request, CancellationToken cancellationToken )
    {
        var result = coolingRoomRepository
            .GetAllWithAudit()
            .MapTo( coolingRoomStatusRepository.GetAll() );
        return Task.FromResult( result );
    }
}