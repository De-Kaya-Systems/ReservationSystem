using DeKayaServer.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure( EntityTypeBuilder<Customer> builder )
    {
        builder.ToTable( "Customers" );
        builder.HasKey( x => x.Id );
        builder.OwnsOne( c => c.FirstName );
        builder.OwnsOne( c => c.LastName );
        builder.OwnsOne( c => c.FullName );
        builder.OwnsOne( c => c.Address );
        builder.OwnsOne( c => c.Contact );
    }
}
