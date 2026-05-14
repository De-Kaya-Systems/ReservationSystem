using DeKayaServer.Domain.CustomerBalance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class CustomerBalanceConfiguration : IEntityTypeConfiguration<CustomerBalance>
{
    public void Configure( EntityTypeBuilder<CustomerBalance> builder )
    {
        builder.ToTable( "CustomerBalances" );
        builder.HasKey( x => x.Id );
        builder.Property( x => x.CustomerId ).IsRequired();
        builder.Property( x => x.SourceType ).IsRequired();
        builder.Property( x => x.SourceId );
        builder.Property( x => x.PaymentTypeId ).IsRequired();

        builder.OwnsOne( x => x.TotalAmount );
        builder.OwnsOne( x => x.OutstandingAmount );
        builder.OwnsOne( x => x.PaidAmount );
        builder.OwnsOne( x => x.AdjustmentAmount );
        builder.OwnsOne( x => x.Description );
        builder.OwnsOne( x => x.BalanceStatus );
        builder.OwnsOne( x => x.LastPaymentAt );

        builder.HasIndex( x => x.CustomerId );
        builder.HasIndex( x => new { x.CustomerId, x.SourceType } );
    }
}
