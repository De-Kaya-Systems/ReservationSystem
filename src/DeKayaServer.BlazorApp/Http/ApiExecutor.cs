using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Http;

/// <summary>
/// UI da her seferinde toast yazmaya gerek yok. Dinamik olarak api sonuçlarını presenter ile gösterir.
/// </summary>
/// <param name="presenter"></param>
public sealed class ApiExecutor(IApiResultPresenter presenter)
{
    public async Task<Result<T>> ExecuteAsync<T>(
      Func<Task<Result<T>>> action,
      ApiPresentationOptions? options = null)
    {
        var result = await action();
        presenter.Present(result, options);
        return result;
    }

    public async Task<Result> ExecuteAsync(
        Func<Task<Result>> action,
        ApiPresentationOptions? options = null)
    {
        var result = await action();
        presenter.Present(result, options);
        return result;
    }
}
