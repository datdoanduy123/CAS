using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// RefreshToken: Dùng để lấy Access Token mới khi cái cũ hết hạn.
    /// Cần lưu DB để có thể thu hồi (Revoke) nếu phát hiện rủi ro.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        public string TokenValue { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid AppId { get; set; }
        public App? App { get; set; }

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        
        // Security Tracking cho Token Rotation
        public string? CreatedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
