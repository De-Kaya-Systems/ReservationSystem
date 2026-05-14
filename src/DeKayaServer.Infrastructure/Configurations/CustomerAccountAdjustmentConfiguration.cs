using DeKayaServer.Domain.CustomerAccountAdjustments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal sealed class CustomerAccountAdjustmentConfiguration
    : IEntityTypeConfiguration<CustomerAccountAdjustment>
{
    public void Configure( EntityTypeBuilder<CustomerAccountAdjustment> builder )
    {
        builder.ToTable( "CustomerAccountAdjustments" );

        builder.HasKey( x => x.Id );

        builder.Property( x => x.CustomerId )
            .IsRequired();

        builder.Property( x => x.CustomerBalanceId )
            .IsRequired();

        builder.Property( x => x.SourceType )
            .IsRequired();

        builder.Property( x => x.Direction )
            .IsRequired();

        builder.Property( x => x.SourceId );

        builder.Property( x => x.AdjustmentType )
            .IsRequired();

        builder.Property( x => x.BalanceBeforeAdjustment )
            .HasPrecision( 18, 2 )
            .IsRequired();

        builder.Property( x => x.BalanceAfterAdjustment )
            .HasPrecision( 18, 2 )
            .IsRequired();

        builder.OwnsOne( x => x.Amount, amount =>
        {
            amount.Property( x => x.Value )
                .HasColumnName( "Amount" )
                .HasPrecision( 18, 2 )
                .IsRequired();
        } );

        builder.OwnsOne( x => x.Reason, reason =>
        {
            reason.Property( x => x.Value )
                .HasColumnName( "Reason" )
                .HasColumnType( "nvarchar(250)" )
                .HasMaxLength( 250 )
                .IsRequired();
        } );

        builder.OwnsOne( x => x.Note, note =>
        {
            note.Property( x => x.Value )
                .HasColumnName( "Note" )
                .HasColumnType( "nvarchar(500)" )
                .HasMaxLength( 500 );
        } );

        builder.HasIndex( x => x.CustomerId );

        builder.HasIndex( x => x.CustomerBalanceId );

        builder.HasIndex( x => new
        {
            x.CustomerId,
            x.AdjustmentType
        } );
    }
}
