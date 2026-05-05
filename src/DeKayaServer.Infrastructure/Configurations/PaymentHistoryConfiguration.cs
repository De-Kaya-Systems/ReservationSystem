using DeKayaServer.Domain.PaymentHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure( EntityTypeBuilder<PaymentHistory> builder )
    {
        builder.ToTable( "PaymentHistories" );
        builder.HasKey( x => x.Id );
        
        builder.Property( x => x.CustomerId ).IsRequired();
        builder.Property( x => x.SourceType ).IsRequired();
        builder.Property( x => x.SourceId );
        builder.Property( x => x.PaymentTypeId ).IsRequired();
        builder.Property( x => x.PaymentAmount ).IsRequired();
        builder.Property( x => x.RemainingBalance ).IsRequired();
        builder.Property( x => x.PaymentDate ).IsRequired();
        builder.Property( x => x.Notes );
        
        builder.HasIndex( x => x.CustomerId );
        builder.HasIndex( x => new { x.CustomerId, x.SourceType } );
        builder.HasIndex( x => x.PaymentDate );
    }
}
