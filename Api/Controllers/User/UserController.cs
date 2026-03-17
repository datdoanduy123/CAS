using Application.DTOs.Common;
using Application.DTOs.Role;
using Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly Application.IServices.User.IUserService _userService;

        public UserController(Application.IServices.User.IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<BaseResponseDTO<bool>> Create([FromBody] CreateUserDTO request)
        {
            return await HandleException(_userService.CreateAsync(request));
        }

        [HttpDelete("{id}")]
        public async Task<BaseResponseDTO<bool>> Delete([FromRoute] Guid id)
        {
            return await HandleException(_userService.DeleteAsync(id));
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<BaseResponseDTO<bool>> SoftDelete([FromRoute] Guid id)
        {
            return await HandleException(_userService.SoftDeleteAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<BaseResponseDTO<bool>> Update([FromRoute] Guid id, [FromBody] UpdateUserDTO request)
        {
            return await HandleException(_userService.UpdateAsync(id, request));
        }

        [HttpGet("{id}")]
        public async Task<BaseResponseDTO<UserListItemDTO?>> GetById([FromRoute] Guid id)
        {
            return await HandleException(_userService.GetByIdAsync(id));
        }

        [HttpGet]
        public async Task<BaseResponseDTO<List<UserListItemDTO>>> GetAll([FromQuery] BaseQueryDTO request, [FromQuery] UserQueryDTO filter)
        {
            var query = new QueryDTO<UserQueryDTO>    
            {
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Query = filter,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            // Gọi service lấy data, Repo đã update query.Total
            var dataTask = _userService.GetAllAsync(query);
            await dataTask; 

            // Sau khi Repo chạy xong, query.Total đã được cập nhật
            var metaData = new MetaDataDTO { Page = query.Page, PageSize = query.PageSize, Total = query.Total };

            return await HandleException(dataTask, metaData);
        }

        // ================================================
        // ROLE ASSIGNMENT - Phân quyền User
        // Giống mô hình Keycloak: Realm Role + App Role
        // ================================================

        /// <summary>
        /// Lấy tất cả quyền (Role) hiện tại của User.
        /// Trả về cả Realm Role (dùng toàn hệ thống) và App Role (riêng từng App).
        /// </summary>
        [HttpGet("{userId}/roles")]
        public async Task<BaseResponseDTO<List<UserRoleDTO>>> GetUserRoles([FromRoute] Guid userId)
        {
            return await HandleException(_userService.GetUserRolesAsync(userId));
        }

        /// <summary>
        /// Gán thêm danh sách Role cho User (không xóa Role cũ).
        /// Body: { "roleIds": ["guid-1", "guid-2"] }
        /// </summary>
        [HttpPost("{userId}/roles")]
        public async Task<BaseResponseDTO<bool>> AssignRoles(
            [FromRoute] Guid userId,
            [FromBody] AssignRolesDTO request)
        {
            return await HandleException(_userService.AssignRolesToUserAsync(userId, request));
        }

        /// <summary>
        /// Đồng bộ toàn bộ Role của User.
        /// Xóa hết quyền cũ, gán lại theo danh sách mới.
        /// Thường dùng cho giao diện checkbox: Admin submit cả trang.
        /// </summary>
        [HttpPut("{userId}/roles")]
        public async Task<BaseResponseDTO<bool>> SyncRoles(
            [FromRoute] Guid userId,
            [FromBody] SyncRolesDTO request)
        {
            return await HandleException(_userService.SyncUserRolesAsync(userId, request));
        }

        /// <summary>
        /// Gỡ bỏ danh sách Role cụ thể khỏi User.
        /// Body: { "roleIds": ["guid-1"] }
        /// </summary>
        [HttpDelete("{userId}/roles")]
        public async Task<BaseResponseDTO<bool>> RemoveRoles(
            [FromRoute] Guid userId,
            [FromBody] AssignRolesDTO request)
        {
            return await HandleException(_userService.RemoveRolesFromUserAsync(userId, request));
        }
    }
}
