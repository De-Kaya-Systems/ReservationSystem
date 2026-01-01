using System.Security.Claims;

namespace DeKayaServer.BlazorApp.Helpers;

public sealed record UserClaims(string? UserId, string? PersonelName, string? FullName, string? Email);
public static class ClaimsPrincipalExtensions
{
    public static UserClaims ToUserClaims(this ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var personelName = user.FindFirst("firstName")?.Value + " " + user.FindFirst("lastName")?.Value;
        var fullName = user.FindFirst("fullName")?.Value;
        var email = user.FindFirst("email")?.Value;

        return new UserClaims(userId, personelName, fullName, email);
    }
}
