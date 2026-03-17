using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppConfiguration : IEntityTypeConfiguration<App>
{
    public void Configure(EntityTypeBuilder<App> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.AppSecret)
            .HasMaxLength(255);

        builder.Property(e => e.RedirectUris)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.LogoutUri)
            .HasMaxLength(500);

        builder.Property(e => e.AllowedGrantTypes)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.AppType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(e => e.Realm)
            .WithMany(r => r.Apps)
            .HasForeignKey(e => e.RealmId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
