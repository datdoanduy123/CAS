using Application.DTOs.Common;
using Application.DTOs.Role;
using Application.DTOs.User;
using Application.IRepositories.Role;
using Application.IRepositories.User;
using Application.IServices.User;
using Application.Helpers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public UserService(IUserRepository userRepository, IUserRoleRepository userRoleRepository)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            return await _userRepository.XoaCung(user);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            user.IsActive = false;
            return await _userRepository.CapNhat(user);
        }

        public async Task<UserListItemDTO?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserListItemDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUserDTO request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (existingEmail != null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.EMAIL_EXIST);
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
            if (request.RealmId.HasValue) user.RealmId = request.RealmId.Value;

            return await _userRepository.CapNhat(user);
        }

        public async Task<bool> CreateAsync(CreateUserDTO request)
        {
            // 1. Kiểm tra tồn tại
            var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUsername != null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_EXIST);

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.EMAIL_EXIST);

            // 2. Hash mật khẩu bằng PBKDF2 với Salt
            var salt = SecurityHelper.GenerateSalt();
            var hashedPassword = SecurityHelper.HashPassword(request.Password, salt);

            // 3. Repo xử lý tạo mới
            return await _userRepository.TaoMoi(request, hashedPassword, salt);
        }

        public async Task<List<UserListItemDTO>> GetAllAsync(QueryDTO<UserQueryDTO> model)
        {
            return await _userRepository.Search(model);
        }

        // =========================================================
        // ROLE ASSIGNMENT - Phân quyền User (giống mô hình Keycloak)
        // =========================================================

        /// <summary>
        /// Lấy danh sách tất cả Role của User.
        /// Bao gồm Realm Role + App Role, kèm thông tin AppId và AppName.
        /// </summary>
        public async Task<List<UserRoleDTO>> GetUserRolesAsync(Guid userId)
        {
            // Kiểm tra User tồn tại
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            return await _userRoleRepository.GetRolesByUserIdAsync(userId);
        }

        /// <summary>
        /// Gán thêm danh sách Role cho User.
        /// Kiểm tra Role hợp lệ trước khi gán. Bỏ qua nếu User đã có Role đó rồi.
        /// </summary>
        public async Task<bool> AssignRolesToUserAsync(Guid userId, AssignRolesDTO request)
        {
            // 1. Kiểm tra User tồn tại
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            if (request.RoleIds == null || request.RoleIds.Count == 0)
                throw new ApplicationException("Danh sách Role không được để trống.");

            // 2. Lọc chỉ lấy RoleId hợp lệ (tồn tại trong DB)
            var validRoleIds = await _userRoleRepository.FilterValidRoleIdsAsync(request.RoleIds);
            if (validRoleIds.Count == 0)
                throw new ApplicationException("Không có Role hợp lệ nào được tìm thấy.");

            // 3. Lấy Role hiện có để tránh gán trùng
            var existingRoles = await _userRoleRepository.GetRolesByUserIdAsync(userId);
            var existingRoleIds = existingRoles.Select(r => r.RoleId).ToHashSet();

            // 4. Chỉ thêm những Role chưa có
            var newUserRoles = validRoleIds
                .Where(roleId => !existingRoleIds.Contains(roleId))
                .Select(roleId => new UserRole { UserId = userId, RoleId = roleId })
                .ToList();

            if (newUserRoles.Count == 0) return true; // Đã có hết, không cần làm gì

            return await _userRoleRepository.AddRangeAsync(newUserRoles);
        }

        /// <summary>
        /// Đồng bộ toàn bộ Role của User.
        /// Xóa hết quyền cũ (nếu có) rồi gán lại theo danh sách mới.
        /// Phù hợp với giao diện Checkbox: Admin chọn xong Submit cả trang.
        /// </summary>
        public async Task<bool> SyncUserRolesAsync(Guid userId, SyncRolesDTO request)
        {
            // 1. Kiểm tra User tồn tại
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            // 2. Xóa toàn bộ Role cũ của User
            await _userRoleRepository.RemoveAllByUserIdAsync(userId);

            // 3. Nếu danh sách mới rỗng thì chỉ cần xóa là xong
            if (request.RoleIds == null || request.RoleIds.Count == 0) return true;

            // 4. Lọc chỉ lấy RoleId hợp lệ (tồn tại trong DB)
            var validRoleIds = await _userRoleRepository.FilterValidRoleIdsAsync(request.RoleIds);

            // 5. Tạo và lưu danh sách UserRole mới
            var newUserRoles = validRoleIds
                .Select(roleId => new UserRole { UserId = userId, RoleId = roleId })
                .ToList();

            if (newUserRoles.Count == 0) return true;

            return await _userRoleRepository.AddRangeAsync(newUserRoles);
        }

        /// <summary>
        /// Gỡ bỏ danh sách Role cụ thể khỏi User.
        /// Chỉ xóa các quyền được chỉ định, các quyền khác giữ nguyên.
        /// </summary>
        public async Task<bool> RemoveRolesFromUserAsync(Guid userId, AssignRolesDTO request)
        {
            // 1. Kiểm tra User tồn tại
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            if (request.RoleIds == null || request.RoleIds.Count == 0)
                throw new ApplicationException("Danh sách Role cần gỡ không được để trống.");

            // 2. Xóa các UserRole tương ứng
            return await _userRoleRepository.RemoveRangeAsync(userId, request.RoleIds);
        }
    }
}
