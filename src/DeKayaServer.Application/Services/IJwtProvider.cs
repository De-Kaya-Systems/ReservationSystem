using DeKayaServer.Domain.Users;

namespace DeKayaServer.Application.Services;

public interface IJwtProvider
{
    Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default);
}
