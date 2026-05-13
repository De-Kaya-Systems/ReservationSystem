using DeKayaServer.Application.Customers;
using DeKayaServer.Contracts.Common;
using DeKayaServer.Contracts.CustomerAccount;
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

        app.MapGet( "list",
            async (
                string? customerName,
                string? phoneNumber,
                int? pageIndex,
                int? pageSize,
                ISender sender,
                CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send(
                    new CustomerListQuery(
                        CustomerName: customerName,
                        PhoneNumber: phoneNumber,
                        PageIndex: pageIndex ?? 0,
                        PageSize: pageSize ?? 25 ),
                    cancellationToken );

                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<PagedResultDto<CustomerListItemDto>>>();

        app.MapGet( "{id}/account",
            async (
                Guid id,
                DateTime? startDate,
                DateTime? endDate,
                ISender sender,
                CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send(
                    new CustomerAccountGetQuery(
                        CustomerId: id,
                        StartDate: startDate,
                        EndDate: endDate ),
                    cancellationToken );

                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<CustomerAccountDto>>();

        app.MapPost( "{id}/payments",
            async (
                Guid id,
                ReceiveCustomerPaymentRequest request,
                ISender sender,
                CancellationToken cancellationToken ) =>
            {
                var command = ReceiveCustomerPaymentCommand.FromRequest(
                    customerId: id,
                    request: request );

                var res = await sender.Send( command, cancellationToken );

                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

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
