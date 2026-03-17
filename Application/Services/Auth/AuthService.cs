using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.IRepositories.Auth;
using Application.IRepositories.User;
using Application.IServices.Auth;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<BaseResponseDTO<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, string appCode, string? ipAddress, string? userAgent)
        {
            // 1. Validate AppCode
            var app = await _authRepository.GetAppByCodeAsync(appCode);
            if (app == null)
            {
                return new BaseResponseDTO<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Mã ứng dụng (AppCode) không hợp lệ hoặc không tồn tại."
                };
            }

            // 2. Find User
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !user.IsActive)
            {
                return new BaseResponseDTO<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Tài khoản hoặc mật khẩu không chính xác."
                };
            }

            // 3. Verify Password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new BaseResponseDTO<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Tài khoản hoặc mật khẩu không chính xác."
                };
            }

            // Cập nhật LastLoginAt
            await _authRepository.UpdateUserLastLoginAsync(user);

            // 4. Create Sessions
            var userSession = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                StartedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddDays(1) // Token expiration
            };

            var appSession = new AppSession
            {
                Id = Guid.NewGuid(),
                UserSessionId = userSession.Id,
                AppId = app.Id,
                StartedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            await _authRepository.SaveSessionsAsync(userSession, appSession);

            // 5. Generate Tokens
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing in appsettings.json");
            var expireMinutes = Convert.ToInt32(jwtSettings["TokenExpirationMinutes"] ?? "60");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("AppCode", app.Code),
                new Claim("UserSessionId", userSession.Id.ToString()),
                new Claim("AppSessionId", appSession.Id.ToString())
            };

            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                {
                    if (ur.Role != null && !string.IsNullOrEmpty(ur.Role.Name))
                    {
                        // Lọc Role: Chỉ lấy Realm Role (AppId == null) hoặc App Role của ứng dụng đang đăng nhập (AppId == app.Id)
                        if (ur.Role.AppId == null || ur.Role.AppId == app.Id)
                        {
                            claims.Add(new Claim("role", ur.Role.Name));
                        }
                    }
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            return new BaseResponseDTO<LoginResponseDTO>
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Data = new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = "dummy-refresh-token", // Thay thế bằng Refresh Token thật sau
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
                    UserSessionId = userSession.Id,
                    AppSessionId = appSession.Id
                }
            };
        }
    }
}
