using DeKayaServer.BlazorApp.Constans;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Services;

public sealed class AuthService(
    HttpClient httpClient,
    IAccessTokenStoreService localStorage
    ) : IAuthService
{
    public async Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostAsJsonAsync(EndpointConstans.LoginEndpoint, loginRequest, cancellationToken);

        var result = await httpResponse.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: cancellationToken)
                     ?? new Result<string>
                     {
                         IsSuccessful = false,
                         StatusCode = (int)httpResponse.StatusCode,
                         ErrorMessages = ["Empty response from server."]
                     };

        return result;
    }

    public async Task<bool> LogOutAsync()
    {
        await localStorage.ClearAsync();
        return true;
    }
}
