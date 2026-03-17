namespace Application.DTOs.Role
{
    /// <summary>
    /// DTO dùng để trả về danh sách Role của 1 User.
    /// Bao gồm phân biệt Realm Role (dùng cho toàn hệ thống) và App Role (dùng riêng cho 1 App).
    /// </summary>
    public class UserRoleDTO
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Nếu null: Realm Role (toàn hệ thống)
        // Nếu có giá trị: App Role (quyền riêng của App đó)
        public Guid? AppId { get; set; }
        public string? AppName { get; set; }
    }

    /// <summary>
    /// DTO dùng khi THÊM hoặc XÓA 1 danh sách Role khỏi User
    /// </summary>
    public class AssignRolesDTO
    {
        public List<Guid> RoleIds { get; set; } = new();
    }

    /// <summary>
    /// DTO dùng khi ĐỒNG BỘ toàn bộ Role của User (xóa cũ, thêm mới theo danh sách)
    /// </summary>
    public class SyncRolesDTO
    {
        public List<Guid> RoleIds { get; set; } = new();
    }
}
