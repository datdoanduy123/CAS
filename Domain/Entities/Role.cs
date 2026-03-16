using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Role: Định nghĩa Quyền (Roles).
    /// Hỗ trợ cả Realm Role (Toàn bộ Server) và Client Role (Quyền riêng của 1 App)
    /// </summary>
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid? RealmId { get; set; }
        public Realm? Realm { get; set; }

        /// <summary>
        /// Nếu ClientId = null, đây là Realm Role (VD: SuperAdmin).
        /// Nếu ClientId có giá trị, đây là Client Role (VD: Editor của App A).
        /// </summary>
        public Guid? ClientId { get; set; }
        public Client? Client { get; set; }

        // Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
