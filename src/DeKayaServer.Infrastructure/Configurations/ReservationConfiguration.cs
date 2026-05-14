using DeKayaServer.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure( EntityTypeBuilder<Reservation> builder )
    {
        builder.ToTable( "Reservations" );

        builder.HasKey( x => x.Id );

        builder.Property( x => x.CustomerId )
            .IsRequired();

        builder.Property( x => x.CoolingRoomId )
            .IsRequired();

        builder.Property( x => x.Status )
            .HasConversion<int>()
            .IsRequired();

        builder.OwnsOne( x => x.DeliveryLocation );
        builder.OwnsOne( x => x.DeliveryDate );
        builder.OwnsOne( x => x.DeliveryTime );
        builder.OwnsOne( x => x.PickUpDate );
        builder.OwnsOne( x => x.PickUpTime );
        builder.OwnsOne( x => x.DeliveredAt );
        builder.OwnsOne( x => x.PickedUpAt );
        builder.OwnsOne( x => x.TotalDay );

        builder.OwnsOne( x => x.CoolingRoomBaseDailyPrice, price =>
        {
            price.Property( x => x.Value )
                .HasColumnType( "decimal(18,2)" )
                .IsRequired();
        } );

        builder.OwnsOne( x => x.CoolingRoomDailyPrice, price =>
        {
            price.Property( x => x.Value )
                .HasColumnType( "decimal(18,2)" )
                .IsRequired();
        } );

        builder.OwnsOne( x => x.PriceOverrideReason, reason =>
        {
            reason.Property( x => x.Value )
                .HasMaxLength( 250 );
        } );

        builder.OwnsOne( x => x.PriceOverrideNote, note =>
        {
            note.Property( x => x.Value )
                .HasMaxLength( 500 );
        } );

        builder.OwnsOne( x => x.ReservationTotalAmount );
        builder.OwnsOne( x => x.PaidAtReservation );
        builder.OwnsOne( x => x.Note );
        builder.OwnsOne( x => x.ReservationNumber );

        builder.HasIndex( x => x.CustomerId );
        builder.HasIndex( x => x.CoolingRoomId );
    }
}
