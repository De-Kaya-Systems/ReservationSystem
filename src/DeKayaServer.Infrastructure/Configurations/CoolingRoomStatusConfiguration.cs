using DeKayaServer.Domain.CoolingRoomStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal sealed class CoolingRoomStatusConfiguration : IEntityTypeConfiguration<CoolingRoomStatus>
{
    public void Configure( EntityTypeBuilder<CoolingRoomStatus> builder )
    {
        builder.ToTable( "CoolingRoomStatuses" );
        builder.HasKey( x => x.Id );

        builder.OwnsOne( x => x.StatusName );
    }
}
