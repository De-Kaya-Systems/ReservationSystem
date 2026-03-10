using DeKayaServer.Application.CoolingRooms;
using DeKayaServer.Contracts.CoolingRooms;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class CoolingRoomModule
{
    public static void MapCoolingRoom( this IEndpointRouteBuilder builder )
    {
        var app = builder
            .MapGroup( "/coolingrooms" )
            .RequireRateLimiting( "fixed" )
            .RequireAuthorization()
            .WithTags( "CoolingRooms" );

        app.MapPost( string.Empty,
            async ( CoolingRoomCreateCommand request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( request, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapPut( string.Empty,
            async ( CoolingRoomUpdateCommand request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( request, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapDelete( "{id}",
            async ( Guid id, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new CoolingRoomDeleteCommand( id ), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapGet( "{id}",
            async ( Guid id, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new CoolingRoomGetQuery( id ), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<CoolingRoomDto>>();
    }
}
