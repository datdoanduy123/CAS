namespace Api.Controllers.User
{
    [Microsoft.AspNetCore.Mvc.ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly Application.IServices.User.IUserService _userService;

        public UserController(Application.IServices.User.IUserService userService)
        {
            _userService = userService;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<Application.DTOs.Common.BaseResponseDTO<List<Application.DTOs.User.UserListItemDTO>>> GetAll([Microsoft.AspNetCore.Mvc.FromQuery] Application.DTOs.Common.BaseQueryDTO request, [Microsoft.AspNetCore.Mvc.FromQuery] Application.DTOs.User.UserQueryDTO filter)
        {
            var query = new Application.DTOs.Common.QueryDTO<Application.DTOs.User.UserQueryDTO>
            {
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Query = filter,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_userService.GetAllAsync(query));
        }
    }
}
