using Application.DTOs.Common;
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
    }
}
