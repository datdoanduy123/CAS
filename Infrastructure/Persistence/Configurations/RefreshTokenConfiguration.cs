using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TokenValue)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.ReplacedByToken)
            .HasMaxLength(255);

        // Đánh index cho Refresh Token
        builder.HasIndex(e => e.TokenValue).IsUnique();

        builder.HasOne(e => e.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Client)
            .WithMany() // Nếu Client không cần List RefreshTokens navigation property
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
