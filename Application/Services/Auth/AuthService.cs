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

            var now = DateTime.UtcNow;

            // 4. Create Sessions
            var userSession = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                StartedAt = now,
                LastAccessedAt = now,
                IsActive = true,
                ExpiresAt = now.AddDays(1) // Token expiration
            };

            var appSession = new AppSession
            {
                Id = Guid.NewGuid(),
                UserSessionId = userSession.Id,
                AppId = app.Id,
                StartedAt = now,
                LastAccessedAt = now
            };

            await _authRepository.SaveSessionsAsync(userSession, appSession);

            // 4.5 Generate Refresh Token
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            string refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenValue = refreshTokenString,
                UserId = user.Id,
                AppId = app.Id,
                ExpiresAt = now.AddDays(7), // Refresh Token valid for 7 days
                CreatedByIp = ipAddress,
                IsRevoked = false
            };

            await _authRepository.SaveRefreshTokenAsync(refreshToken);

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
                Expires = now.AddMinutes(expireMinutes),
                NotBefore = now,
                IssuedAt = now,
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
                    RefreshToken = refreshTokenString, 
                    ExpiresAt = now.AddMinutes(expireMinutes),
                    RefreshTokenExpiresAt = refreshToken.ExpiresAt,
                    UserSessionId = userSession.Id,
                    AppSessionId = appSession.Id
                }
            };
        }

        public async Task<BaseResponseDTO<LoginResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, string appCode, string? ipAddress, string? userAgent)
        {
            // 1. Fetch Refresh Token
            var refreshToken = await _authRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null || refreshToken.App?.Code != appCode)
            {
                return new BaseResponseDTO<LoginResponseDTO> { Success = false, Message = "Refresh Token không hợp lệ hoặc không thuộc ứng dụng này." };
            }

            if (!refreshToken.IsActive || refreshToken.User == null || !refreshToken.User.IsActive || refreshToken.App == null || !refreshToken.App.IsActive)
            {
                return new BaseResponseDTO<LoginResponseDTO> { Success = false, Message = "Refresh Token đã hết hạn / bị thu hồi hoặc tài khoản không hoạt động." };
            }

            // 2. Revoke old token
            refreshToken.IsRevoked = true;
            await _authRepository.UpdateRefreshTokenAsync(refreshToken);

            // 3. Generate New Refresh Token
            var now = DateTime.UtcNow;
            
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            string newRefreshTokenString = Convert.ToBase64String(randomNumber);

            var newRefreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenValue = newRefreshTokenString,
                UserId = refreshToken.UserId,
                AppId = refreshToken.AppId,
                ExpiresAt = now.AddDays(7),
                CreatedByIp = ipAddress,
                ReplacedByToken = newRefreshTokenString,
                IsRevoked = false
            };

            await _authRepository.SaveRefreshTokenAsync(newRefreshToken);

            // 4. Generate New Access Token
            var user = refreshToken.User;
            var app = refreshToken.App;

            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing");
            var expireMinutes = Convert.ToInt32(jwtSettings["TokenExpirationMinutes"] ?? "60");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("AppCode", app.Code),
                // Session IDs will be empty since RefreshToken entity doesn't trace them natively without deeper DB schemas.
                new Claim("UserSessionId", Guid.Empty.ToString()),
                new Claim("AppSessionId", Guid.Empty.ToString())
            };

            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                {
                    if (ur.Role != null && !string.IsNullOrEmpty(ur.Role.Name))
                    {
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
                Expires = now.AddMinutes(expireMinutes),
                NotBefore = now,
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // 5. Return updated DTO
            return new BaseResponseDTO<LoginResponseDTO>
            {
                Success = true,
                Message = "Refresh Token thành công",
                Data = new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshTokenString, 
                    ExpiresAt = now.AddMinutes(expireMinutes),
                    RefreshTokenExpiresAt = newRefreshToken.ExpiresAt,
                    UserSessionId = Guid.Empty,
                    AppSessionId = Guid.Empty
                }
            };
        }

        public async Task<BaseResponseDTO<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO request)
        {
            // 1. Fetch User
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new BaseResponseDTO<string>
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại hoặc đã bị khóa."
                };
            }

            // 2. Verify Old Password
            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
            {
                return new BaseResponseDTO<string>
                {
                    Success = false,
                    Message = "Mật khẩu hiện tại không chính xác."
                };
            }

            // 3. Hash New Password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, salt);

            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = salt;

            // 4. Update Database
            await _authRepository.UpdateUserPasswordAsync(user);

            return new BaseResponseDTO<string>
            {
                Success = true,
                Message = "Đổi mật khẩu thành công.",
                Data = "OK"
            };
        }
    }
}
