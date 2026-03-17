using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.IServices.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<BaseResponseDTO<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            // 1. Lấy X-App-Code từ Header
            if (!Request.Headers.TryGetValue("X-App-Code", out var appCodeValues) || string.IsNullOrWhiteSpace(appCodeValues.ToString()))
            {
                return new BaseResponseDTO<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Thiếu thông tin X-App-Code trên Header."
                };
            }
            string appCode = appCodeValues.ToString();

            // 2. Lấy IP & UserAgent
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? userAgent = Request.Headers["User-Agent"].ToString();

            // 3. Gọi Service
            var loginTask = _authService.LoginAsync(request, appCode, ipAddress, userAgent);
            
            // Re-use HandleException từ BaseController nếu cần, nhưng login trả về BaseResponseDTO rồi
            return await loginTask;
        }
    }
}
