using DeKayaServer.Application.CoolingRooms;
using DeKayaServer.Contracts.CoolingRooms;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class CoolingRoomMaintenanceModule
{
    public static void MapCoolingRoomMaintenance( this IEndpointRouteBuilder builder )
    {
        var app = builder
            .MapGroup( "/coolingroommaintenances" )
            .RequireRateLimiting( "fixed" )
            .RequireAuthorization()
            .WithTags( "CoolingRoomMaintenances" );

        app.MapGet( string.Empty,
            async ( ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new CoolingRoomMaintenanceGetAllQuery(), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<List<CoolingRoomMaintenanceLogDto>>>();
    }
}