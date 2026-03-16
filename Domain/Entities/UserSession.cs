using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// UserSession: Phiên đăng nhập TỔNG của 1 User trên hệ thống CAS.
    /// Nếu Session này bị hủy, toàn bộ ClientSession bên dưới cũng bay màu (Logout).
    /// </summary>
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<ClientSession> ClientSessions { get; set; } = new List<ClientSession>();
    }
}
