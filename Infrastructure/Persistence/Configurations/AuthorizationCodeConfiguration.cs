using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuthorizationCodeConfiguration : IEntityTypeConfiguration<AuthorizationCode>
{
    public void Configure(EntityTypeBuilder<AuthorizationCode> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.RedirectUri)
            .IsRequired()
            .HasMaxLength(1000);

        // Đánh index cho Code để tìm nhanh lúc đổi Token
        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasOne(e => e.App)
            .WithMany(c => c.AuthorizationCodes)
            .HasForeignKey(e => e.AppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany(u => u.AuthorizationCodes)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
