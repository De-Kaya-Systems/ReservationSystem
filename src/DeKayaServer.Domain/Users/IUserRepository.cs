using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.Users;

public interface IUserRepository : IAuditableRepository<User>
{
}
