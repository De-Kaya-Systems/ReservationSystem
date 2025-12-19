using DeKayaServer.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DeKayaServer.Infrastructure;

public static class ExtensionMethods
{
    //Extensıon method şu işlemi yapar: ModelBuilder üzerindeki tüm entity türlerini dolaşır ve eğer tür Entity sınıfından türemişse, o entity için global bir sorgu filtresi uygular.
    ///Bu filtre, IsDeleted özelliği false olan kayıtları döndürür, yani silinmiş kayıtlar sorgulardan otomatik olarak hariç tutulur.
    //EN: The extension method does the following: It iterates over all entity types on the ModelBuilder and, if the type is derived from the Entity class, it applies a global query filter for that entity.
    //This filter returns records where the IsDeleted property is false, meaning that deleted records are automatically excluded from queries.
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(Entity).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }
}