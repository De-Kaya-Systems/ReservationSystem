using DeKayaServer.Application.Reservations;
using DeKayaServer.Contracts.Common;
using DeKayaServer.Contracts.Reservations;
using DeKayaServer.Contracts.Reservations.Enum;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class ReservationModule
{
    public static void MapReservation( this IEndpointRouteBuilder builder )
    {
        var app = builder
            .MapGroup( "/reservations" )
            .RequireRateLimiting( "fixed" )
            .RequireAuthorization()
            .WithTags( "Reservation" );

        app.MapGet( "list",
            async (
                string? customerName,
                DateOnly? reservationStartDate,
                DateOnly? reservationEndDate,
                ReservationOperationFilterDto? operationStatus,
                int? pageIndex,
                int? pageSize,
                ISender sender,
                CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send(
                    new ReservationListQuery(
                        CustomerName: customerName,
                        ReservationStartDate: reservationStartDate,
                        ReservationEndDate: reservationEndDate,
                        OperationStatus: operationStatus,
                        PageIndex: pageIndex ?? 0,
                        PageSize: pageSize ?? 25 ),
                    cancellationToken );

                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<PagedResultDto<ReservationListItemDto>>>();

        app.MapPost( string.Empty,
            async ( ReservationCreateCommand request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( request, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapPut( string.Empty,
            async ( ReservationUpdateCommand request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( request, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapPut( "{id}/complete",
            async ( Guid id, CompleteReservationRequest request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var command = new ReservationCompleteCommand(
                    Id: id,
                    DeliveredAt: request.DeliveredAt,
                    PickedUpAt: request.PickedUpAt,
                    PaymentReceived: request.PaymentReceived,
                    PaymentTypeId: request.PaymentTypeId,
                    PaymentAmount: request.PaymentAmount,
                    HasFault: request.HasFault,
                    FaultDescription: request.FaultDescription,
                    Note: request.Note );

                var res = await sender.Send( command, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapPut( "{id}/mark-delivered",
            async ( Guid id, MarkReservationDeliveredRequest request, ISender sender, CancellationToken cancellationToken ) =>
            {
                var command = new ReservationMarkAsDeliveredCommand(
                    Id: id,
                    DeliveredAt: request.DeliveredAt );

                var res = await sender.Send( command, cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapDelete( "{id}",
            async ( Guid id, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new ReservationDeleteCommand( id ), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<string>>();

        app.MapGet( "{id}",
            async ( Guid id, ISender sender, CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send( new ReservationGetQuery( id ), cancellationToken );
                return res.IsSuccessful ? Results.Ok( res ) : Results.InternalServerError( res );
            } )
            .Produces<Result<ReservationDto>>();
    }
}
