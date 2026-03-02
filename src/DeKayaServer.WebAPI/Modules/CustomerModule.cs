using DeKayaServer.Application.Customers;
using DeKayaServer.Contracts.Customers;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class CustomerModule
{
    public static void MapCustomer( this IEndpointRouteBuilder builder )
    {
        var app = builder
            .MapGroup( "/customers" )
            .RequireRateLimiting( "fixed" )
            .RequireAuthorization()
            .WithTags( "Customers" );

        app.MapPost( string.Empty,
            async ( CustomerCreateCommand request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( request, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapPut( string.Empty,
            async ( CustomerUpdateCommand request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( request, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapDelete( "{id}",
            async ( Guid id, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new CustomerDeleteCommand( id ), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapGet( "{id}",
            async ( Guid id, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new CustomerGetQuery( id ), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<CustomerDto>>();
    }
}
