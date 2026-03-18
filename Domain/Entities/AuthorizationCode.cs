using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// AuthorizationCode: Đoạn mã hash ngắn hạn dùng trong Authorization Code Flow
    /// User Login xong => SSO tạo Code => App cầm Code đi đổi Token
    /// </summary>
    public class AuthorizationCode : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        public Guid AppId { get; set; }
        public App? App { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public string RedirectUri { get; set; } = string.Empty;
        
        // Thời gian sống của Code rất ngắn (VD: 1-5 phút)
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
