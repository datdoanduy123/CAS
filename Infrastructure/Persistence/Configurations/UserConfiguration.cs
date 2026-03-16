using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Entity Framework cho bảng User
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.PasswordHash)
            .IsRequired();
            
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(e => e.FullName)
            .HasMaxLength(150);
    }
}
