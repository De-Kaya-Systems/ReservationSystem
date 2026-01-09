using DeKayaServer.Domain.LoginTokens;
using DeKayaServer.Infrastructure.Context;
using GenericRepository;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class LoginTokenRepository : Repository<LoginToken, ApplicationDbContext>, ILoginTokenRepository
{
    public LoginTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
