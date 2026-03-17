using Application.DTOs.Role;
using Domain.Entities;

namespace Application.IRepositories.Role
{
    /// <summary>
    /// Repository interface cho bảng UserRole (Gán quyền cho User).
    /// Tách riêng khỏi IUserRepository để đúng chuẩn Single Responsibility.
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Lấy danh sách Role hiện tại của User (kèm thông tin App nếu là App Role)
        /// </summary>
        Task<List<UserRoleDTO>> GetRolesByUserIdAsync(Guid userId);

        /// <summary>
        /// Thêm danh sách UserRole mới (bỏ qua nếu đã tồn tại)
        /// </summary>
        Task<bool> AddRangeAsync(List<UserRole> userRoles);

        /// <summary>
        /// Xóa các UserRole theo UserId và danh sách RoleId cần gỡ
        /// </summary>
        Task<bool> RemoveRangeAsync(Guid userId, List<Guid> roleIds);

        /// <summary>
        /// Xóa toàn bộ Role của User, dùng trước khi Sync lại
        /// </summary>
        Task<bool> RemoveAllByUserIdAsync(Guid userId);

        /// <summary>
        /// Kiểm tra RoleId có tồn tại trong DB không (tránh gán Role rác)
        /// </summary>
        Task<List<Guid>> FilterValidRoleIdsAsync(List<Guid> roleIds);
    }
}
