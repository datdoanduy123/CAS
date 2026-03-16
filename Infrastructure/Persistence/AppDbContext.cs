using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Lớp AppDbContext đóng vai trò giao tiếp chính giữa ứng dụng và cơ sở dữ liệu.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Bảng Users trong CSDL
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;
    
    // SSO Entities
    public DbSet<Realm> Realms { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<ClientSession> ClientSessions { get; set; } = null!;
    
    public DbSet<AuthorizationCode> AuthorizationCodes { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Load tất cả các cấu hình (IEntityTypeConfiguration) từ assembly hiện tại (Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
