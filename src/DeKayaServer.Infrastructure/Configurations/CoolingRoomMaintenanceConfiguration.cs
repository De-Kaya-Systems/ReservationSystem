using DeKayaServer.Domain.CoolingRoomMaintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class CoolingRoomMaintenanceConfiguration : IEntityTypeConfiguration<CoolingRoomMaintenance>
{
    public void Configure( EntityTypeBuilder<CoolingRoomMaintenance> builder )
    {
        builder.ToTable( "CoolingRoomMaintenances" );
        builder.HasKey( x => x.Id );
        builder.OwnsOne( m => m.Description );
        builder.OwnsOne( m => m.MaintenanceDateStart );
        builder.OwnsOne( m => m.MaintenanceDateEnd );
    }
}
