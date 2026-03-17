using Application.DTOs.Common;
using Application.DTOs.Role;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices.User
{
    public interface IUserService
    {
        Task<List<UserListItemDTO>> GetAllAsync(QueryDTO<UserQueryDTO> model);
        Task<bool> CreateAsync(CreateUserDTO request);
        Task<bool> UpdateAsync(Guid id, UpdateUserDTO request);
        Task<UserListItemDTO?> GetByIdAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);

        // ==========================================
        // ROLE ASSIGNMENT - Phân quyền User
        // ==========================================

        /// <summary>
        /// Lấy danh sách toàn bộ Role hiện có của 1 User (Realm Role + App Role)
        /// </summary>
        Task<List<UserRoleDTO>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Gán thêm 1 danh sách Role cho User (không xóa Role cũ)
        /// </summary>
        Task<bool> AssignRolesToUserAsync(Guid userId, AssignRolesDTO request);

        /// <summary>
        /// Đồng bộ lại toàn bộ Role của User - Xóa hết cũ, gán theo danh sách mới
        /// Dùng cho giao diện Checkbox (Submit cả trang)
        /// </summary>
        Task<bool> SyncUserRolesAsync(Guid userId, SyncRolesDTO request);

        /// <summary>
        /// Gỡ bỏ 1 danh sách Role cụ thể khỏi User
        /// </summary>
        Task<bool> RemoveRolesFromUserAsync(Guid userId, AssignRolesDTO request);
    }
}
