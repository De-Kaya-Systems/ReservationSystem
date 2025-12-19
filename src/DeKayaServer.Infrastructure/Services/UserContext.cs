using DeKayaServer.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DeKayaServer.Infrastructure.Services;

internal sealed class UserContext(
    HttpContextAccessor httpContextAccessor) : IUserContext
{
    // Bu method, mevcut kullanıcının kimliğini (ID) alır ve bir GUID olarak döndürür. Eğer kullanıcı kimliği bulunamazsa veya geçerli bir GUID değilse, uygun istisnalar fırlatır.
    // EN: This method retrieves the current user's identity (ID) and returns it as a GUID. If the user ID cannot be found or is not a valid GUID, it throws appropriate exceptions.
    public Guid GetUserId()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var claims = httpContext.User.Claims;
        string? userId = claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new ArgumentNullException("User ID claim not found.");
        }

        try
        {
            Guid id = Guid.Parse(userId);
            return id;
        }
        catch (Exception)
        {
            throw new ArgumentException("User ID claim is not a valid GUID.");
        }
    }
}
