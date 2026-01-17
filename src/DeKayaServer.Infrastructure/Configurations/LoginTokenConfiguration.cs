using DeKayaServer.Domain.LoginTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal sealed class LoginTokenConfiguration : IEntityTypeConfiguration<LoginToken>
{
    public void Configure(EntityTypeBuilder<LoginToken> builder)
    {
        builder.ToTable("LoginTokens");
        builder.HasKey(x => x.Id);
        builder.OwnsOne(x => x.Token);
        builder.OwnsOne(x => x.ExpiresDate);
        builder.OwnsOne(x => x.IsActive);
    }
}
