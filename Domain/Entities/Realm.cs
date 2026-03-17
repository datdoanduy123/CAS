using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Realm: Quản lý không gian (Tenancy/Realm) để cô lập dữ liệu
    /// </summary>
    public class Realm : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<App> Apps { get; set; } = new List<App>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
