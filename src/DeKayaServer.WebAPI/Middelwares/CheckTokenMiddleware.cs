using DeKayaServer.Domain.LoginTokens;
using System.Security.Claims;

namespace DeKayaServer.WebAPI.Middelwares;

/// <summary>
/// IMiddleware bize ihtiyacımız olan middleware yapısını sağlar. Bu middleware, gelen isteklerdeki token'ları kontrol etmek için kullanilır.
/// EN: IMiddleware provides us with the necessary middleware structure. This middleware is used to check tokens in incoming requests.
/// </summary>
public sealed class CheckTokenMiddleware(
    ILoginTokenRepository loginTokenRepository) : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (string.IsNullOrWhiteSpace(token))
        {
            await next(httpContext);
            return;
        }

        var userId = httpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
            .Value;
        if (userId is null)
        {
            throw new TokenException();
        }

        var isTokenAvailable = await loginTokenRepository.AnyAsync(p =>
            p.UserId == userId
            && p.Token.Value == token
            && p.IsActive.Value == true);
        if (!isTokenAvailable)
        {
            throw new TokenException();
        }

        await next(httpContext);
    }
}

public sealed class TokenException : Exception;