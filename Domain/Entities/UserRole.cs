using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// UserRole: Bảng trung gian Map N-N giữa User và Role
    /// Lưu ý: Không kế thừa BaseEntity để setup Composite Key: (UserId, RoleId)
    /// </summary>
    public class UserRole
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
