using DeKayaServer.Application.Dashboard;
using DeKayaServer.Contracts.Dashboard;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class DashboardModule
{
    public static void MapDashboard( this IEndpointRouteBuilder builder )
    {
        var app = builder
            .MapGroup( "/dashboard" )
            .RequireRateLimiting( "fixed" )
            .RequireAuthorization()
            .WithTags( "Dashboard" );

        app.MapGet( "summary",
            async (
                ISender sender,
                CancellationToken cancellationToken ) =>
            {
                var res = await sender.Send(
                    new DashboardSummaryGetQuery(),
                    cancellationToken );

                return res.IsSuccessful
                    ? Results.Ok( res )
                    : Results.InternalServerError( res );
            } )
            .Produces<Result<DashboardSummaryDto>>();
    }
}
