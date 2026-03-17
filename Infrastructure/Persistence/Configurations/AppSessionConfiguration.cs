using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppSessionConfiguration : IEntityTypeConfiguration<AppSession>
{
    public void Configure(EntityTypeBuilder<AppSession> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.UserSession)
            .WithMany(us => us.AppSessions)
            .HasForeignKey(e => e.UserSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.App)
            .WithMany(c => c.AppSessions)
            .HasForeignKey(e => e.AppId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
