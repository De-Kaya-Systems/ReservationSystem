using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Users;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace DeKayaServer.Infrastructure.Abstractions;

/// <summary>
/// Audit işlemleri için genişletilmiş repository. Böylece audit bilgilerini çekmek için her seferinde join yazmamak mümkün olur.
/// EN: Extended repository for audit operations. This way, it is possible to avoid writing joins every time to fetch audit information.
/// </summary>

internal class AuditableRepository<TEntity, TContext>(TContext context) : Repository<TEntity, TContext>(context), IAuditableRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    private readonly TContext _context = context;

    public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit()
    {

        var entities = _context.Set<TEntity>().AsNoTracking().AsQueryable();
        var users = _context.Set<User>().AsNoTracking().AsQueryable();

        var res = entities
           .Join(users, x => x.CreatedBy, x => x.Id, (y, user) =>
               new { entity = y, createdUser = user })
           .GroupJoin(users, x => x.entity.UpdatedBy, x => x.Id, (y, user) =>
               new { y.entity, y.createdUser, updatedUser = user })
           .SelectMany(z => z.updatedUser.DefaultIfEmpty(),
               (z, updateUser) => new EntityWithAuditDto<TEntity>
               {
                   Entity = z.entity,
                   CreatedUser = z.createdUser,
                   UpdatedUser = updateUser
               });

        return res;
    }
}
