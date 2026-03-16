using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ClientSessionConfiguration : IEntityTypeConfiguration<ClientSession>
{
    public void Configure(EntityTypeBuilder<ClientSession> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.UserSession)
            .WithMany(us => us.ClientSessions)
            .HasForeignKey(e => e.UserSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.ClientSessions)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
