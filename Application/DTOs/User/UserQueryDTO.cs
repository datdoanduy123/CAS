namespace Application.DTOs.User
{
    public class UserQueryDTO
    {
        public bool? IsActive { get; set; }
        // Thêm các tiêu chí lọc khác nếu cần (ví dụ: RealmId)
        public System.Guid? RealmId { get; set; }
    }
}
