using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Entity Framework cho bảng UserRole
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {        
        // Cấu hình Composite Key (Khóa chính kết hợp)
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        
        // Mối quan hệ N-1 với User
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Nếu xóa User thì xóa luôn quyền tương ứng
            
        // Mối quan hệ N-1 với Role
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade); // Nếu xóa Role thì xóa luôn bảng map
    }
}
