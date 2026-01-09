using DeKayaServer.BlazorApp.Models;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginRequest loginRequest, CancellationToken ct = default);
    Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task<Result<string>> ResetPasswordAsync(string forgotPasswordCode, string newPassword, CancellationToken ct = default);
    Task<Result<bool>> CheckForgotPasswordCodeAsync(string forgotPasswordCode, CancellationToken ct = default);
    Task<bool> LogOutAsync();
}
