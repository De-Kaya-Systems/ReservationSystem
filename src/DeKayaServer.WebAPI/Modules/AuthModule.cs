using DeKayaServer.Application.Auth;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.WebAPI.Modules;

public static class AuthModule
{
    public static void MapAuth(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/auth").WithTags("Auth");

        app.MapPost("/login",
            async (LoginCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(request, cancellationToken);
                return result.IsSuccessful
                ? Results.Ok(result)
                : Results.InternalServerError(result);
            }).Produces<Result<string>>()
            .RequireRateLimiting("login-fixed"); ;

        app.MapPost("/forgotpassword/{email}",
            async (string email, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ForgotPasswordCommand(email), cancellationToken);
                return result.IsSuccessful
                ? Results.Ok(result)
                : Results.InternalServerError(result);
            }).Produces<Result<string>>()
            .RequireRateLimiting("forgot-password-fixed");

        app.MapPost("/reset-password",
            async (ResetPasswordCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(request, cancellationToken);
                return result.IsSuccessful
                ? Results.Ok(result)
                : Results.InternalServerError(result);
            }).Produces<Result<string>>()
            .RequireRateLimiting("reset-password-fixed");

        app.MapGet("/check-forgot-password-code/{forgotPasswordCode}",
        async (Guid forgotPasswordCode, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CheckForgotPasswordCodeCommand(forgotPasswordCode), cancellationToken);
            return result.IsSuccessful
            ? Results.Ok(result)
            : Results.InternalServerError(result);
        }).Produces<Result<string>>()
        .RequireRateLimiting("check-forgot-password-code-fixed");
    }
}
