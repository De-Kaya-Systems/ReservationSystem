using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Users;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace DeKayaServer.Infrastructure.Abstractions;

/// <summary>
/// Audit işlemleri için genişletilmiş repository. Böylece audit bilgilerini çekmek için her seferinde join yazmamak mümkün olur.
/// EN: Extended repository for audit operations. This way, it is possible to avoid writing joins every time to fetch audit information.
/// </summary>

internal class AuditableRepository<TEntity, TContext>( TContext context ) : Repository<TEntity, TContext>( context ), IAuditableRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    private readonly TContext _context = context;

    public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit()
    {
        var entities = _context.Set<TEntity>().AsQueryable();
        var users = _context.Set<User>().AsNoTracking().AsQueryable();

        var res = entities
          .Join( users, m => m.CreatedBy, m => m.Id, ( b, user ) =>
                  new { entity = b, createdUser = user } )
          .GroupJoin( users, m => m.entity.UpdatedBy, m => m.Id, ( b, user ) =>
                  new { b.entity, b.createdUser, updatedUser = user } )
          .SelectMany( s => s.updatedUser.DefaultIfEmpty(),
              ( x, updatedUser ) => new EntityWithAuditDto<TEntity>
              {
                  Entity = x.entity,
                  CreatedUser = x.createdUser,
                  UpdatedUser = updatedUser
              } );

        return res;
    }
}
