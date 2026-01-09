using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IApiResultPresenter
{
    void Present(Result result, ApiPresentationOptions? options = null);
}
