using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.IServices.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
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

        [HttpPost("refresh-token")]
        public async Task<BaseResponseDTO<LoginResponseDTO>> RefreshToken([FromBody] RefreshTokenRequestDTO request)
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
            return await _authService.RefreshTokenAsync(request, appCode, ipAddress, userAgent);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<BaseResponseDTO<string>> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return new BaseResponseDTO<string>
                {
                    Success = false,
                    Message = "Không thể xác định danh tính người dùng."
                };
            }

            return await _authService.ChangePasswordAsync(userId, request);
        }

        [HttpPost("forgot-password")]
        public async Task<BaseResponseDTO<string>> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            return await _authService.ForgotPasswordAsync(request);
        }

        [HttpPost("verify-otp")]
        public async Task<BaseResponseDTO<string>> VerifyOtp([FromBody] VerifyOtpRequestDTO request)
        {
            return await _authService.VerifyOtpAsync(request);
        }

        [HttpPost("reset-password")]
        public async Task<BaseResponseDTO<string>> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            return await _authService.ResetPasswordAsync(request);
        }
    }
}
