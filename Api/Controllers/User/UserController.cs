using Application.DTOs.Common;
using Application.DTOs.User;
using Application.IServices.User;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers.User
{
    [ApiController]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Tạo người dùng mới trong hệ thống.
        /// </summary>
        /// <param name="request">Thông tin tạo user</param>
        /// <returns>Thông tin user vừa được tạo</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponseDTO<CreateUserResponseDTO>), 200)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDTO request)
        {
            if (!ModelState.IsValid)
                return Ok(BaseResponseDTO<CreateUserResponseDTO>.FailResponse("Invalid request data.", 400));

            var result = await _userService.CreateUserAsync(request);
            return Ok(result);
        }
    }
}
