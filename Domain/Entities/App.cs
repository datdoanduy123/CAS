using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// App: Đại diện cho các ứng dụng vệ tinh kết nối đến dự án SSO (CAS)
    /// </summary>
    public class App : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        
        public string Code { get; set; } = string.Empty;    
        
        /// <summary>
        /// Mật khẩu của ứng dụng (dùng cho luồng bảo mật cao)
        /// </summary>
        public string? AppSecret { get; set; }
        
        /// <summary>
        /// Danh sách URL được phép Redirect về sau khi Login thành công. 
        /// Thường lưu dưới dạng chuỗi phân cách dấu phẩy hoặc JSON
        /// </summary>
        public string RedirectUris { get; set; } = string.Empty;

        /// <summary>
        /// Ví dụ: "authorization_code", "App_credentials", "refresh_token"
        /// </summary>
        public string AllowedGrantTypes { get; set; } = string.Empty;
        
        /// <summary>
        /// URL để SSO gọi về khi User văng Session tổng (Back-channel logout)
        /// </summary>
        public string? LogoutUri { get; set; }

        /// <summary>
        /// Phân loại: Public (spa/mobile) hay Confidential (web server giữ secret)
        /// </summary>
        public string AppType { get; set; } = "Confidential";

        public bool IsActive { get; set; } = true;

        // Cho phép App thuộc về 1 Realm cụ thể (tuỳ chọn)
        public Guid? RealmId { get; set; }
        public Realm? Realm { get; set; }

        // Navigation Properties
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<AppSession> AppSessions { get; set; } = new List<AppSession>();
        public ICollection<AuthorizationCode> AuthorizationCodes { get; set; } = new List<AuthorizationCode>();
    }
}
