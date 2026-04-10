using DeKayaServer.Domain.PaymentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal sealed class PaymentTypeConfiguration : IEntityTypeConfiguration<PaymentTypes>
{
    public void Configure( EntityTypeBuilder<PaymentTypes> builder )
    {
        builder.ToTable( "PaymentTypes" );
        builder.HasKey( x => x.Id );

        builder.OwnsOne( x => x.PaymentType );
    }
}
