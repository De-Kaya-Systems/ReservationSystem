using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Users;

namespace DeKayaServer.Application;

/// <summary>
/// Bunu method yazmamın sebebi Audit bilgilerini çekmek için her seferinde join yazmamak. Böylece kod tekrarı önlenmiş olur. Daha sonra bu methodu tekrar değerlendireceğim lazım mı değil mi diye.
/// EN: The reason I wrote this method is to avoid writing joins every time to fetch audit information. This prevents code duplication. I will reevaluate this method later to see if it's needed or not.
/// </summary>
internal static class ExtensionMethods
{
    public static IQueryable<EntityWithAuditDto<TEntity>> ApplyAuditDto<TEntity>
        (this IQueryable<TEntity> entities, IQueryable<User> users)
        where TEntity : Entity
    {
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
