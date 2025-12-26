using DeKayaServer.Domain.Users;
using DeKayaServer.Infrastructure.Context;
using GenericRepository;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User, ApplicationDbContext>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
}
