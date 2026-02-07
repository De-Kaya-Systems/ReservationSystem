using DeKayaServer.BlazorApp.Models;
using TS.Result;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IApiResultPresenter
{
    void Present<T>( Result<T> result, ApiPresentationOptions? options = null );
}
