using DeKayaServer.Domain.Users;

namespace DeKayaServer.Application.Services;

public interface IJwtProvider
{
    string CreateToken(User user);
}
