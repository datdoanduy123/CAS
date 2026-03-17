using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(e => e.Realm)
            .WithMany(r => r.Roles)
            .HasForeignKey(e => e.RealmId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.App)
            .WithMany(c => c.Roles)
            .HasForeignKey(e => e.AppId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
