using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public sealed class ToastApiResultPresenter( ToastService toast ) : IApiResultPresenter
{
    public void Present<T>( Result<T> result, ApiPresentationOptions? options = null )
    {
        options ??= new ApiPresentationOptions();

        if ( result.IsSuccessful )
        {
            if ( !options.ShowSuccess )
                return;
            //Success mesaji goster. 1-eger options.SuccessMessage varsa onu goster. 2- eger result string ise ve data'si varsa onu goster. 3- degilse default success mesaji goster.
            //EN: Show success message. 1- if options.SuccessMessage exists, show it. 2- if result is string and has data, show it. 3- otherwise show default success message.
            var message = options.SuccessMessage;
            if ( string.IsNullOrWhiteSpace( message ) && result is Result<string> s && !string.IsNullOrWhiteSpace( s.Data ) )
            {
                message = s.Data;
            }

            message ??= ToastMessageConstants.Success;
            toast.ShowSuccess( message );
            return;

        }
        if ( !options.ShowError )
            return;

        var title = options.ErrorTitle ?? ToastMessageConstants.Error;
        var errors = result.ErrorMessages?
            .Where( x => !string.IsNullOrWhiteSpace( x ) )
            .ToList();

        if ( errors is { Count: > 0 } )
        {
            if ( options.CollapseMultipleErrors )
            {
                toast.ShowError( $"{title}: {errors[ 0 ]}" );
            }
            else
            {
                foreach ( var error in errors )
                {
                    toast.ShowError( $"{title}: {error}" );
                }
            }
            return;
        }

        // Fallback: ErrorMessages yoksa status code ile mesaj üret
        // EN: Fallback: If there are no ErrorMessages, generate message with status code
        var fallback = result.StatusCode switch
        {
            400 => "Geçersiz istek yapıldı.",
            401 => "Yetkisiz erişim. Lütfen giriş yapın.",
            403 => "Erişim engellendi.",
            404 => "İstenen kaynak bulunamadı.",
            408 => "İstek zaman aşımına uğradı.",
            422 => "İstek tamamlanmadı. Girdiğiniz verileri kontrol edin.",
            500 => "Sunucu hatası oluştu.",
            503 => "Sunucu hizmet veremiyor.",
            _ => $"Bir hata oluştu. Hata kodu: {result.StatusCode}"
        };
        toast.ShowError( $"{title}: {fallback}" );
    }
}
