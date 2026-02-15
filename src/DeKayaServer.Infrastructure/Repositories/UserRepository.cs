using DeKayaServer.Domain.Users;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class UserRepository : AuditableRepository<User, ApplicationDbContext>, IUserRepository
{
    public UserRepository( ApplicationDbContext context ) : base( context )
    {
    }
}
