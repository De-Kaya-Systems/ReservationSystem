using DeKayaServer.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeKayaServer.Infrastructure.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.OwnsOne(u => u.FirstName);
        builder.OwnsOne(u => u.LastName);
        builder.OwnsOne(u => u.FullName);
        builder.OwnsOne(u => u.Email);
        builder.OwnsOne(u => u.UserName);
        builder.OwnsOne(u => u.Password);
        builder.OwnsOne(u => u.ForgotPasswordCode);
        builder.OwnsOne(u => u.ForgotPasswordDate);
        builder.OwnsOne(u => u.IsForgotPasswordCompleted);
    }
}
