using DeKayaServer.BlazorApp.Constants;
using System.Security.Claims;
using System.Text.Json;

namespace DeKayaServer.BlazorApp.Helpers;

public sealed record UserClaims(
    string? UserId,
    string? PersonelName,
    string? FullName,
    string? Email,
    string? Role,
    IReadOnlyList<string> Permissions )
{
    public bool IsSysAdmin => string.Equals( Role, RoleConstants.SysAdmin, StringComparison.OrdinalIgnoreCase );
    public bool HasPermission( string permission )
        => IsSysAdmin || ( !string.IsNullOrWhiteSpace( permission ) &&
             Permissions.Any( p => string.Equals( p, permission, StringComparison.OrdinalIgnoreCase ) ) );
};
public static class ClaimsPrincipalExtensions
{
    public static UserClaims ToUserClaims( this ClaimsPrincipal user )
    {
        var userId = user.FindFirst( ClaimTypes.NameIdentifier )?.Value;
        var personelName = user.FindFirst( ClaimTypeConstants.FirstName )?.Value + " " + user.FindFirst( ClaimTypeConstants.LastName )?.Value;
        var fullName = user.FindFirst( ClaimTypeConstants.FullName )?.Value;
        var email = user.FindFirst( ClaimTypeConstants.Email )?.Value;
        var role = user.FindFirst( ClaimTypeConstants.Role )?.Value;
        var permissions = ReadPermissions( user );

        return new UserClaims( userId, personelName, fullName, email, role, permissions );
    }

    private static IReadOnlyList<string> ReadPermissions( ClaimsPrincipal user )
    {
        var raw = user.FindFirst( ClaimTypeConstants.Permissions )?.Value;
        if ( string.IsNullOrEmpty( raw ) )
        {
            return [];
        }

        try
        {
            var list = JsonSerializer.Deserialize<List<string>>( raw );
            return ( list ?? [] )
                .Where( x => !string.IsNullOrWhiteSpace( x ) )
                .Select( x => x.Trim() )
                .Distinct( StringComparer.OrdinalIgnoreCase )
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
