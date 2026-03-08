using DeKayaServer.Domain.CoolingRooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class CoolingRoomConfiguration : IEntityTypeConfiguration<CoolingRoom>
{
    public void Configure( EntityTypeBuilder<CoolingRoom> builder )
    {
        builder.ToTable( "CoolingRooms" );
        builder.HasKey( x => x.Id );
        builder.OwnsOne( r => r.RoomName );
        builder.OwnsOne( r => r.DailyPrice, p =>
        {
            p.Property( r => r.Value ).HasColumnType( "decimal(18,2)" );
        } );
        builder.OwnsOne( r => r.Shelf );
    }
}
