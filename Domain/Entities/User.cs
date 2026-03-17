using System;
using System.Collections.Generic;
using Domain.Common;
namespace Domain.Entities;

/// <summary>
/// Thực thể User đại diện cho người dùng hệ thống.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    
    public string? FullName { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // SSO Navigation Properties
    public Guid? RealmId { get; set; }
    public Realm? Realm { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public ICollection<AuthorizationCode> AuthorizationCodes { get; set; } = new List<AuthorizationCode>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
