using DeKayaServer.Domain.Role;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class RoleRepository(ApplicationDbContext context) : AuditableRepository<Role, ApplicationDbContext>(context), IRoleRepository
{
}
