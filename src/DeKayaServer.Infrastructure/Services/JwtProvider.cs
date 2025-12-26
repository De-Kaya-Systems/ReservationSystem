using DeKayaServer.Application.Services;
using DeKayaServer.Domain.Users;

namespace DeKayaServer.Infrastructure.Services;

internal sealed class JwtProvider : IJwtProvider
{
    public string CreateToken(User user)
    {
        return "token";
    }
}
