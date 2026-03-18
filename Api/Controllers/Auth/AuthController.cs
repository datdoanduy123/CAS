using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.IServices.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        // Cookie name stored on the CAS domain to track SSO session
        private const string SsoCookieName = "cas_session";

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Existing Direct-Login Endpoints
        // ══════════════════════════════════════════════════════════════════════════

        [HttpPost("login")]
        public async Task<BaseResponseDTO<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            if (!Request.Headers.TryGetValue("X-App-Code", out var appCodeValues) || string.IsNullOrWhiteSpace(appCodeValues.ToString()))
                return new BaseResponseDTO<LoginResponseDTO> { Success = false, Message = "Thiếu thông tin X-App-Code trên Header." };

            string appCode = appCodeValues.ToString();
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? userAgent = Request.Headers["User-Agent"].ToString();

            return await _authService.LoginAsync(request, appCode, ipAddress, userAgent);
        }

        [HttpPost("refresh-token")]
        public async Task<BaseResponseDTO<LoginResponseDTO>> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            if (!Request.Headers.TryGetValue("X-App-Code", out var appCodeValues) || string.IsNullOrWhiteSpace(appCodeValues.ToString()))
                return new BaseResponseDTO<LoginResponseDTO> { Success = false, Message = "Thiếu thông tin X-App-Code trên Header." };

            string appCode = appCodeValues.ToString();
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? userAgent = Request.Headers["User-Agent"].ToString();

            return await _authService.RefreshTokenAsync(request, appCode, ipAddress, userAgent);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<BaseResponseDTO<string>> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return new BaseResponseDTO<string> { Success = false, Message = "Không thể xác định danh tính người dùng." };

            return await _authService.ChangePasswordAsync(userId, request);
        }

        [HttpPost("forgot-password")]
        public async Task<BaseResponseDTO<string>> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
            => await _authService.ForgotPasswordAsync(request);

        [HttpPost("verify-otp")]
        public async Task<BaseResponseDTO<string>> VerifyOtp([FromBody] VerifyOtpRequestDTO request)
            => await _authService.VerifyOtpAsync(request);

        [HttpPost("reset-password")]
        public async Task<BaseResponseDTO<string>> ResetPassword([FromBody] ResetPasswordRequestDTO request)
            => await _authService.ResetPasswordAsync(request);

        // ══════════════════════════════════════════════════════════════════════════
        // SSO Authorization Code Flow Endpoints
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Entry point for every App redirect.
        /// Checks the HttpOnly SSO cookie (cas_session):
        ///   - Found &amp; valid  → Phase 2 SSO bypass: creates AuthCode for App-B, redirects directly.
        ///   - Not found      → Phase 1: redirects browser to GET /login with all params.
        /// </summary>
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize(
            [FromQuery] string appCode,
            [FromQuery] string redirectUri)
        {
            // Check for a valid SSO cookie on the CAS domain
            if (Request.Cookies.TryGetValue(SsoCookieName, out var sessionIdStr) &&
                Guid.TryParse(sessionIdStr, out var sessionId))
            {
                // Phase 2: existing UserSession → attempt SSO bypass
                var ssoResult = await _authService.AuthorizeWithSessionAsync(
                    sessionId, appCode, redirectUri);

                if (ssoResult.Success && ssoResult.Data != null)
                {
                    // Redirect browser directly back to App with code (no login page shown!)
                    return Redirect(ssoResult.Data.RedirectUri);
                }
                // Session cookie present but expired/invalid → fall through to login
            }

            // Phase 1 (or expired SSO): redirect to login form with all context preserved
            var loginUrl = $"/api/auth/login-form" +
                           $"?appCode={Uri.EscapeDataString(appCode)}" +
                           $"&redirectUri={Uri.EscapeDataString(redirectUri)}";

            return Redirect(loginUrl);
        }

        /// <summary>
        /// GET /api/auth/login-form
        /// Returns the login form metadata so the App (or CAS UI) can render the login page.
        /// All SSO context params are echoed back as hidden fields for the form to POST to /api/auth/sso-login.
        /// </summary>
        [HttpGet("login-form")]
        public IActionResult GetLoginForm(
            [FromQuery] string appCode,
            [FromQuery] string redirectUri)
        {
            // Return a JSON payload with the POST target and all hidden fields.
            // The frontend/SPA uses this to build and display the form.
            return Ok(new
            {
                postUrl = "/api/auth/sso-login",
                fields = new
                {
                    appCode,
                    redirectUri
                }
            });
        }

        /// <summary>
        /// POST /api/auth/sso-login
        /// User submits credentials here (Phase 1).
        /// On success:
        ///   - Sets HttpOnly SSO cookie (cas_session = UserSession.Id)
        ///   - Returns redirect URL: redirectUri?code=AUTH_CODE
        /// </summary>
        [HttpPost("sso-login")]
        public async Task<IActionResult> SsoLogin([FromBody] SsoLoginRequestDTO request)
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.AuthorizeWithCredentialsAsync(request, ipAddress, userAgent);

            if (!result.Success || result.Data == null)
                return BadRequest(new { result.Success, result.Message });

            // Set the HttpOnly SSO cookie so the browser carries UserSessionId for future /authorize calls
            if (result.Data.UserSessionId.HasValue)
            {
                Response.Cookies.Append(SsoCookieName, result.Data.UserSessionId.Value.ToString(),
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });
            }

            // Return the redirect URL (frontend performs the redirect)
            return Ok(new
            {
                result.Success,
                result.Message,
                redirectUri = result.Data.RedirectUri,
                code = result.Data.Code
            });
        }

        /// <summary>
        /// POST /api/auth/token
        /// Server-to-server: App-A/B backend exchanges the auth code for scoped tokens.
        /// Validates: grant_type, Client ID, Client Secret, RedirectUri, PKCE.
        /// </summary>
        [HttpPost("token")]
        public async Task<BaseResponseDTO<LoginResponseDTO>> Token([FromBody] SsoTokenRequestDTO request)
            => await _authService.ExchangeCodeForTokenAsync(request);
    }
}
