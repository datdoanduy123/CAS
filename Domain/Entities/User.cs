using System;

namespace Domain.Entities;

/// <summary>
/// Thực thể User đại diện cho người dùng hệ thống.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    
    public string? FullName { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
