using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// ClientSession: Đại diện cho 1 phiên đăng nhập của 1 User trên 1 App cụ thể.
    /// Nó nằm trong 1 UserSession tổng của SSO.
    /// </summary>
    public class ClientSession : BaseEntity
    {
        public Guid UserSessionId { get; set; }
        public UserSession? UserSession { get; set; }

        public Guid ClientId { get; set; }
        public Client? Client { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    }
}
