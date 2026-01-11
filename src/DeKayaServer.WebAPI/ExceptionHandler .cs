using DeKayaServer.Application.Behaviors;
using DeKayaServer.WebAPI.Middelwares;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using TS.Result;

namespace DeKayaServer.WebAPI;

///<summary>
///ExceptionHandler sınıfı, uygulama genelinde oluşan istisnaları yakalayarak uygun HTTP yanıtlarını döndürmek için kullanılır.
///ve böylece hatalar daha kullanıcı dostu bir şekilde yönetilir. Kullanıcıya anlamlı hata mesajları sağlanır.
///EN: The ExceptionHandler class is used to catch exceptions that occur throughout the application and return appropriate HTTP responses,
///and thus errors are managed in a more user-friendly way. It provides meaningful error messages to the user.
///</summary>

public sealed class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        Result<string> errorResult;

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = 500;

        ///<summary>
        ///İç içe AggregateException durumunda, gerçek hatayı almak için InnerException kullanılır.
        ///En: In case of nested AggregateException, InnerException is used to get the actual error.
        ///</summary>
        var actualException = exception is AggregateException agg && agg.InnerException != null
        ? agg.InnerException
        : exception;

        var exceptionType = actualException.GetType();
        var validationExceptionType = typeof(ValidationException);
        var authorizationExceptionType = typeof(AuthorizationException);
        var tokenException = typeof(TokenException);

        ///<summary>
        ///ValidationException durumunda, HTTP 422 (Unprocessable Entity) durumu döndürülür ve hatalar liste halinde kullanıcıya iletilir.
        ///EN: In case of ValidationException, HTTP 422 (Unprocessable Entity) status is returned and errors are communicated to the user in a list.
        ///</summary>
        if (exceptionType == validationExceptionType)
        {
            httpContext.Response.StatusCode = 422;

            errorResult = Result<string>.Failure(422, ((ValidationException)exception).Errors.Select(s => s.PropertyName).ToList());

            await httpContext.Response.WriteAsJsonAsync(errorResult);

            return true;
        }

        ///<summary>
        ///AuthorizationException durumunda, HTTP 403 (Forbidden) durumu döndürülür ve kullanıcıya yetkisiz işlem mesajı iletilir.
        ///EN: In case of AuthorizationException, HTTP 403 (Forbidden) status is returned and an unauthorized operation message is communicated to the user.
        ///</summary>
        if (exceptionType == authorizationExceptionType)
        {
            httpContext.Response.StatusCode = 403;
            errorResult = Result<string>.Failure(403, "Bu işlem için yetkiniz yok");
            await httpContext.Response.WriteAsJsonAsync(errorResult);
            return true;
        }

        ///<summary>
        /// TokenException durumunda, HTTP 401 (Unauthorized) durumu döndürülür ve kullanıcıya geçersiz token mesajı iletilir.
        /// EN: In case of TokenException, HTTP 401 (Unauthorized) status is returned and an invalid token message is communicated to the user.
        ///</summary>
        if (exceptionType == tokenException)
        {
            httpContext.Response.StatusCode = 401;
            errorResult = Result<string>.Failure(401, "Token geçersiz");
            await httpContext.Response.WriteAsJsonAsync(errorResult);
            return true;
        }

        errorResult = Result<string>.Failure(exception.Message);

        await httpContext.Response.WriteAsJsonAsync(errorResult);

        return true;
    }
}
