using DeKayaServer.Application.Auth;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class AuthModule
{
    public static void MapAuth(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/auth").RequireRateLimiting("login-fixed");
        app.MapPost("/login",
            async (LoginCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(request, cancellationToken);
                return result.IsSuccessful
                ? Results.Ok(result)
                : Results.InternalServerError(result);
            }).
            Produces<Result<string>>();
    }
}
