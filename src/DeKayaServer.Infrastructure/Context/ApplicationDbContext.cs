using DeKayaServer.Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Claims;

namespace DeKayaServer.Infrastructure.Context;

//Dışardan erişim gerekmiyor.Onun için internal yaptım.
//EN: No external access is required. Therefore, I made it internal.
internal sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ApplyConfigurationsFromAssembly metodu, belirtilen derlemedeki tüm IEntityTypeConfiguration<> uygulamalarını otomatik olarak bulur ve uygular.
        //EN: The ApplyConfigurationsFromAssembly method automatically finds and applies all IEntityTypeConfiguration<> implementations in the specified assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        //ApplyGlobalFilters, tüm varlıklar için küresel sorgu filtrelerini uygulamak üzere kullanılan özel bir genişletme metodudur.
        //EN: ApplyGlobalFilters is a custom extension method used to apply global query filters for all entities.
        modelBuilder.ApplyGlobalFilters();
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<IdentityId>().HaveConversion<IdentityIdValueConverter>();
        configurationBuilder.Properties<decimal>().HaveColumnType("decimal(18,2)");
        configurationBuilder.Properties<string>().HaveColumnType("varchar(MAX)");
        base.ConfigureConventions(configurationBuilder);
    }

    // Burada kod şu işlemi yapılıyor: SaveChangesAsync metodu çağrıldığında, değişiklik izleyicisindeki tüm Entity türündeki varlıklar üzerinde döner. 
    //Her bir varlık için, ekleme, güncelleme veya silme işlemlerine göre ilgili tarih ve kullanıcı bilgilerini otomatik olarak ayarlar.
    // EN: The code here does the following: When the SaveChangesAsync method is called, it iterates over all entities of type Entity in the change tracker.
    // For each entity, it automatically sets the relevant date and user information based on whether the operation is an addition, update, or deletion.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>();

        HttpContextAccessor httpContextAccessor = new();
        string userIdString =
            httpContextAccessor
            .HttpContext!
            .User
            .Claims
            .First(p => p.Type == ClaimTypes.NameIdentifier)
            .Value;

        Guid userId = Guid.Parse(userIdString);
        IdentityId identityId = new(userId);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(p => p.CreatedAt)
                    .CurrentValue = DateTimeOffset.Now;
                entry.Property(p => p.CreatedBy)
                    .CurrentValue = identityId;
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Property(p => p.IsDeleted).CurrentValue == true)
                {
                    entry.Property(p => p.DeletedAt)
                    .CurrentValue = DateTimeOffset.Now;
                    entry.Property(p => p.DeletedBy)
                    .CurrentValue = identityId;
                }
                else
                {
                    entry.Property(p => p.UpdatedAt)
                        .CurrentValue = DateTimeOffset.Now;
                    entry.Property(p => p.UpdatedBy)
                    .CurrentValue = identityId;
                }
            }

            if (entry.State == EntityState.Deleted)
            {
                throw new ArgumentException("Db'den direkt silme işlemi yapamazsınız");
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class IdentityIdValueConverter : ValueConverter<IdentityId, Guid>
{
    public IdentityIdValueConverter() : base(m => m.Value, m => new IdentityId(m)) { }
}