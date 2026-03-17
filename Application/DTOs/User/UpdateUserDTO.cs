using System;

namespace Application.DTOs.User
{
    public class UpdateUserDTO
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool? IsActive { get; set; }
        public Guid? RealmId { get; set; }
        // Password thường sẽ có API đổi mật khẩu riêng, nhưng nếu muốn gộp thì thêm ở đây
    }
}
