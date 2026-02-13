using DeKayaServer.Domain.Role;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure( EntityTypeBuilder<Role> builder )
    {
        builder.ToTable( "Roles" );
        builder.HasKey( x => x.Id );
        builder.OwnsOne( r => r.Name );
        builder.OwnsMany( i => i.Permissions );
    }
}
