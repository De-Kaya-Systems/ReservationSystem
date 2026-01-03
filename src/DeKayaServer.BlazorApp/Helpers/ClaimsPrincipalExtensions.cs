using DeKayaServer.BlazorApp.Constans;
using System.Security.Claims;

namespace DeKayaServer.BlazorApp.Helpers;

public sealed record UserClaims(string? UserId, string? PersonelName, string? FullName, string? Email);
public static class ClaimsPrincipalExtensions
{
    public static UserClaims ToUserClaims(this ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var personelName = user.FindFirst(ClaimTypeConstants.FirstName)?.Value + " " + user.FindFirst(ClaimTypeConstants.LastName)?.Value;
        var fullName = user.FindFirst(ClaimTypeConstants.FullName)?.Value;
        var email = user.FindFirst(ClaimTypeConstants.Email)?.Value;

        return new UserClaims(userId, personelName, fullName, email);
    }
}
