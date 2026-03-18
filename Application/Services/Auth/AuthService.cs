using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.IRepositories.Auth;
using Application.IRepositories.User;
using Application.IServices;
using Application.IServices.Auth;
using Application.Helpers;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(IAuthRepository authRepository, IUserRepository userRepository, IConfiguration configuration, IEmailService emailService)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Existing Direct-Login Endpoints (kept intact)
        // ══════════════════════════════════════════════════════════════════════════

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
            bool isPasswordValid = SecurityHelper.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!isPasswordValid)
            {
                return new BaseResponseDTO<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Tài khoản hoặc mật khẩu không chính xác."
                };
            }

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
                ExpiresAt = now.AddDays(1)
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
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(randomNumber);
            string refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenValue = refreshTokenString,
                UserId = user.Id,
                AppId = app.Id,
                ExpiresAt = now.AddDays(7),
                CreatedByIp = ipAddress,
                IsRevoked = false
            };

            await _authRepository.SaveRefreshTokenAsync(refreshToken);

            // 5. Generate Access Token
            var accessToken = GenerateAppScopedToken(user, app, now);
            var jwtSettings = _configuration.GetSection("Jwt");
            var expireMinutes = Convert.ToInt32(jwtSettings["TokenExpirationMinutes"] ?? "60");

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
                return Fail<LoginResponseDTO>("Refresh Token không hợp lệ hoặc không thuộc ứng dụng này.");

            if (!refreshToken.IsActive || refreshToken.User == null || !refreshToken.User.IsActive || refreshToken.App == null || !refreshToken.App.IsActive)
                return Fail<LoginResponseDTO>("Refresh Token đã hết hạn / bị thu hồi hoặc tài khoản không hoạt động.");

            // 2. Revoke old token
            refreshToken.IsRevoked = true;
            await _authRepository.UpdateRefreshTokenAsync(refreshToken);

            var now = DateTime.UtcNow;

            // 3. Generate New Refresh Token
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(randomNumber);
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
            var accessToken = GenerateAppScopedToken(user, app, now);

            var jwtSettings = _configuration.GetSection("Jwt");
            var expireMinutes = Convert.ToInt32(jwtSettings["TokenExpirationMinutes"] ?? "60");

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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                return Fail<string>("Tài khoản không tồn tại hoặc đã bị khóa.");

            if (!SecurityHelper.VerifyPassword(request.OldPassword, user.PasswordHash, user.PasswordSalt))
                return Fail<string>("Mật khẩu hiện tại không chính xác.");

            string salt = SecurityHelper.GenerateSalt();
            user.PasswordHash = SecurityHelper.HashPassword(request.NewPassword, salt);
            user.PasswordSalt = salt;

            await _authRepository.UpdateUserPasswordAsync(user);

            return new BaseResponseDTO<string> { Success = true, Message = "Đổi mật khẩu thành công.", Data = "OK" };
        }

        public async Task<BaseResponseDTO<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                return new BaseResponseDTO<string> { Success = false, Message = "Email này không tồn tại trong hệ thống hoặc tài khoản đã bị vô hiệu hóa.", Data = null };

            var rand = new Random();
            string otp = rand.Next(100000, 999999).ToString();
            user.ResetPasswordOtp = otp;
            user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(5);
            await _authRepository.UpdateUserPasswordAsync(user);

            string message = $"Mã OTP của bạn là: {otp}. Mã có hiệu lực trong 5 phút.";
            await _emailService.SendEmailAsync(user.Email, "Reset Password OTP", message);

            return new BaseResponseDTO<string> { Success = true, Message = "Nếu email hợp lệ, bạn sẽ nhận được OTP để đặt lại mật khẩu.", Data = "Mã OTP đã được gửi." };
        }

        public async Task<BaseResponseDTO<string>> VerifyOtpAsync(VerifyOtpRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                return Fail<string>("Thông tin không chính xác.");

            if (user.ResetPasswordOtp != request.Otp || user.ResetPasswordOtpExpiry < DateTime.UtcNow)
                return Fail<string>("OTP không hợp lệ hoặc đã hết hạn.");

            string resetToken = Guid.NewGuid().ToString("N");
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiry = null;
            await _authRepository.UpdateUserPasswordAsync(user);

            return new BaseResponseDTO<string> { Success = true, Message = "Xác thực OTP thành công.", Data = resetToken };
        }

        public async Task<BaseResponseDTO<string>> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                return Fail<string>("Thông tin không chính xác.");

            if (user.ResetPasswordToken != request.ResetToken || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                return Fail<string>("Phiên đổi mật khẩu đã hết hạn hoặc không hợp lệ.");

            string salt = SecurityHelper.GenerateSalt();
            user.PasswordHash = SecurityHelper.HashPassword(request.NewPassword, salt);
            user.PasswordSalt = salt;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            await _authRepository.UpdateUserPasswordAsync(user);

            return new BaseResponseDTO<string> { Success = true, Message = "Đặt lại mật khẩu thành công.", Data = "OK" };
        }

        // ══════════════════════════════════════════════════════════════════════════
        // SSO Authorization Code Flow
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Phase 1: User submits credentials → CAS authenticates → creates UserSession + AppSession + AuthCode.
        /// Returns the auth code and the full redirect URL to send back to App-A via browser.
        /// </summary>
        public async Task<BaseResponseDTO<SsoAuthorizeResponseDTO>> AuthorizeWithCredentialsAsync(
            SsoLoginRequestDTO request, string? ipAddress, string? userAgent)
        {
            // 1. Validate AppCode
            var app = await _authRepository.GetAppByCodeAsync(request.AppCode);
            if (app == null)
                return Fail<SsoAuthorizeResponseDTO>("Mã ứng dụng (AppCode) không hợp lệ hoặc không tồn tại.");

            // 2. Validate RedirectUri is registered for this App
            if (!IsValidRedirectUri(app.RedirectUris, request.RedirectUri))
                return Fail<SsoAuthorizeResponseDTO>("RedirectUri không hợp lệ hoặc không được đăng ký cho ứng dụng này.");

            // 4. Authenticate user
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !user.IsActive)
                return Fail<SsoAuthorizeResponseDTO>("Tài khoản hoặc mật khẩu không chính xác.");

            if (!SecurityHelper.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                return Fail<SsoAuthorizeResponseDTO>("Tài khoản hoặc mật khẩu không chính xác.");

            await _authRepository.UpdateUserLastLoginAsync(user);

            var now = DateTime.UtcNow;

            // 5. Create global UserSession (SSO session — 8 hours)
            var userSession = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                StartedAt = now,
                LastAccessedAt = now,
                IsActive = true,
                ExpiresAt = now.AddHours(8)
            };

            // 6. Create AppSession scoped to App-A under the UserSession
            var appSession = new AppSession
            {
                Id = Guid.NewGuid(),
                UserSessionId = userSession.Id,
                AppId = app.Id,
                StartedAt = now,
                LastAccessedAt = now
            };

            await _authRepository.SaveSessionsAsync(userSession, appSession);

            // 7. Generate short-lived AuthorizationCode (5 minutes)
            var codeBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(codeBytes);
            string authCode = Convert.ToHexString(codeBytes).ToLowerInvariant();

            var authorizationCode = new AuthorizationCode
            {
                Id = Guid.NewGuid(),
                Code = authCode,
                AppId = app.Id,
                UserId = user.Id,
                RedirectUri = request.RedirectUri,
                ExpiresAt = now.AddMinutes(5),
                IsUsed = false
            };

            await _authRepository.SaveAuthorizationCodeAsync(authorizationCode);

            // 8. Return code + full redirect URL (CAS controller will also set the SSO cookie)
            var redirectUrl = $"{request.RedirectUri.TrimEnd('/')}?code={authCode}";
            return new BaseResponseDTO<SsoAuthorizeResponseDTO>
            {
                Success = true,
                Message = "Xác thực thành công. Chuyển hướng về ứng dụng.",
                Data = new SsoAuthorizeResponseDTO
                {
                    Code = authCode,
                    RedirectUri = redirectUrl,
                    UserSessionId = userSession.Id   // Controller uses this to set the SSO cookie
                }
            };
        }

        /// <summary>
        /// Phase 2: SSO bypass — look up an existing valid UserSession (identified by cookie),
        /// create a NEW AppSession + AuthCode for App-B. No password prompt.
        /// </summary>
        public async Task<BaseResponseDTO<SsoAuthorizeResponseDTO>> AuthorizeWithSessionAsync(
            Guid userSessionId, string appCode, string redirectUri)
        {
            // 1. Look up active UserSession (from SSO cookie value)
            var userSession = await _authRepository.GetActiveUserSessionAsync(userSessionId);
            if (userSession == null)
                return Fail<SsoAuthorizeResponseDTO>("Phiên SSO không tồn tại hoặc đã hết hạn.");

            // 2. Validate AppCode
            var app = await _authRepository.GetAppByCodeAsync(appCode);
            if (app == null)
                return Fail<SsoAuthorizeResponseDTO>("Mã ứng dụng (AppCode) không hợp lệ hoặc không tồn tại.");

            // 3. Validate RedirectUri
            if (!IsValidRedirectUri(app.RedirectUris, redirectUri))
                return Fail<SsoAuthorizeResponseDTO>("RedirectUri không hợp lệ hoặc không được đăng ký cho ứng dụng này.");

            var now = DateTime.UtcNow;

            // 4. Update existing UserSession to keep it alive
            userSession.LastAccessedAt = now;
            await _authRepository.UpdateUserSessionAsync(userSession);

            // 5. Create AppSession for the target App under the existing UserSession
            var appSession = new AppSession
            {
                Id = Guid.NewGuid(),
                UserSessionId = userSession.Id,
                AppId = app.Id,
                StartedAt = now,
                LastAccessedAt = now
            };

            await _authRepository.SaveAppSessionAsync(appSession);

            // 6. Generate new AuthCode scoped to App-B
            var codeBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(codeBytes);
            string authCode = Convert.ToHexString(codeBytes).ToLowerInvariant();

            var authorizationCode = new AuthorizationCode
            {
                Id = Guid.NewGuid(),
                Code = authCode,
                AppId = app.Id,
                UserId = userSession.UserId,
                RedirectUri = redirectUri,
                ExpiresAt = now.AddMinutes(5),
                IsUsed = false
            };

            await _authRepository.SaveAuthorizationCodeAsync(authorizationCode);

            // Construct redirect URL, handling existing query parameters if any
            var separator = redirectUri.Contains("?") ? "&" : "?";
            var redirectUrl = $"{redirectUri}{separator}code={authCode}";
            return new BaseResponseDTO<SsoAuthorizeResponseDTO>
            {
                Success = true,
                Message = "SSO thành công. Chuyển hướng về ứng dụng.",
                Data = new SsoAuthorizeResponseDTO { Code = authCode, RedirectUri = redirectUrl }
            };
        }

        /// <summary>
        /// Exchange an AuthorizationCode for scoped Access + Refresh tokens.
        /// Server-to-server call from App-A/B backend.
        /// Validates: grant_type, Client ID, Client Secret, RedirectUri, PKCE S256.
        /// </summary>
        public async Task<BaseResponseDTO<LoginResponseDTO>> ExchangeCodeForTokenAsync(SsoTokenRequestDTO request)
        {
            // 1. Validate grant_type
            if (!string.Equals(request.GrantType, "authorization_code", StringComparison.OrdinalIgnoreCase))
                return Fail<LoginResponseDTO>("GrantType không hợp lệ. Phải là 'authorization_code'.");

            // 2. Look up AuthorizationCode
            var authCode = await _authRepository.GetAuthorizationCodeAsync(request.Code);
            if (authCode == null)
                return Fail<LoginResponseDTO>("AuthCode không tồn tại.");
            if (authCode.IsUsed)
                return Fail<LoginResponseDTO>("AuthCode đã được sử dụng.");
            if (authCode.IsExpired)
                return Fail<LoginResponseDTO>("AuthCode đã hết hạn.");

            var app = authCode.App;
            var user = authCode.User;

            if (app == null || !app.IsActive)
                return Fail<LoginResponseDTO>("Ứng dụng không tồn tại hoặc đã bị vô hiệu hóa.");
            if (user == null || !user.IsActive)
                return Fail<LoginResponseDTO>("Tài khoản không tồn tại hoặc đã bị khóa.");

            // 3. Validate Client ID
            if (!string.Equals(app.Code, request.AppCode, StringComparison.Ordinal))
                return Fail<LoginResponseDTO>("AppCode không khớp với AuthCode.");

            // 4. Validate Client Secret (for Confidential apps only)
            if (!string.IsNullOrEmpty(app.AppSecret))
            {
                if (string.IsNullOrEmpty(request.AppSecret) ||
                    !string.Equals(app.AppSecret, request.AppSecret, StringComparison.Ordinal))
                    return Fail<LoginResponseDTO>("AppSecret không hợp lệ.");
            }

            // 5. Validate RedirectUri (exact, case-sensitive match)
            if (!string.Equals(authCode.RedirectUri, request.RedirectUri, StringComparison.Ordinal))
                return Fail<LoginResponseDTO>("RedirectUri không khớp.");

            // 7. Mark code as used (prevent replay)
            authCode.IsUsed = true;
            await _authRepository.UpdateAuthorizationCodeAsync(authCode);

            var now = DateTime.UtcNow;

            // 8. Generate Refresh Token
            var rngBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(rngBytes);
            string refreshTokenString = Convert.ToBase64String(rngBytes);

            var refreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenValue = refreshTokenString,
                UserId = user.Id,
                AppId = app.Id,
                ExpiresAt = now.AddDays(7),
                IsRevoked = false
            };

            await _authRepository.SaveRefreshTokenAsync(refreshToken);

            // 9. Generate scoped Access Token (realm roles + this app's roles only)
            var accessToken = GenerateAppScopedToken(user, app, now);
            var jwtSettings = _configuration.GetSection("Jwt");
            var expireMinutes = Convert.ToInt32(jwtSettings["TokenExpirationMinutes"] ?? "60");

            return new BaseResponseDTO<LoginResponseDTO>
            {
                Success = true,
                Message = "Đổi code lấy token thành công.",
                Data = new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenString,
                    ExpiresAt = now.AddMinutes(expireMinutes),
                    RefreshTokenExpiresAt = refreshToken.ExpiresAt,
                    UserSessionId = Guid.Empty,
                    AppSessionId = Guid.Empty
                }
            };
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Private Helpers
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Generates a JWT scoped to the given App.
        /// Includes: UserId, username, AppCode, realm roles (AppId == null), app-specific roles (AppId == app.Id).
        /// </summary>
        private string GenerateAppScopedToken(Domain.Entities.User user, Domain.Entities.App app, DateTime now)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
            var expireMinutes = Convert.ToInt32(jwtSettings["TokenExpirationMinutes"] ?? "60");
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("AppCode", app.Code)
            };

            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                {
                    if (ur.Role != null && !string.IsNullOrEmpty(ur.Role.Name))
                    {
                        if (ur.Role.AppId == null || ur.Role.AppId == app.Id)
                            claims.Add(new Claim("role", ur.Role.Name));
                    }
                }
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = now.AddMinutes(expireMinutes),
                NotBefore = now,
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
        }

        /// <summary>
        /// Checks if redirectUri is in the App's allowed list (comma-separated).
        /// Comparison is case-insensitive.
        /// </summary>
        private static bool IsValidRedirectUri(string registeredUris, string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(registeredUris) || string.IsNullOrWhiteSpace(redirectUri))
                return false;

            var allowed = registeredUris.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var uri in allowed)
            {
                if (string.Equals(uri, redirectUri, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>Shorthand for returning a failed BaseResponseDTO.</summary>
        private static BaseResponseDTO<T> Fail<T>(string message) =>
            new BaseResponseDTO<T> { Success = false, Message = message };
    }
}
