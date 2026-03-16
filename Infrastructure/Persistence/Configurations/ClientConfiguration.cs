using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.ClientSecret)
            .HasMaxLength(255);

        builder.Property(e => e.RedirectUris)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.LogoutUri)
            .HasMaxLength(500);

        builder.Property(e => e.AllowedGrantTypes)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ClientType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(e => e.Realm)
            .WithMany(r => r.Clients)
            .HasForeignKey(e => e.RealmId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
