using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Role: Định nghĩa Quyền (Roles).
    /// Hỗ trợ cả Realm Role (Toàn bộ Server) và App Role (Quyền riêng của 1 App)
    /// </summary>
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid? RealmId { get; set; }
        public Realm? Realm { get; set; }

        /// <summary>
        /// Nếu AppId = null, đây là Realm Role (VD: SuperAdmin).
        /// Nếu AppId có giá trị, đây là App Role (VD: Editor của App A).
        /// </summary>
        public Guid? AppId { get; set; }
        public App? App { get; set; }

        // Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
