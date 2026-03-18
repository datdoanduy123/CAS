using Application.DTOs.Auth;
using Application.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace Application.IServices.Auth
{
    public interface IAuthService
    {
        // ── Existing direct-login endpoints (kept intact) ──────────────────────
        Task<BaseResponseDTO<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, string appCode, string? ipAddress, string? userAgent);
        Task<BaseResponseDTO<LoginResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, string appCode, string? ipAddress, string? userAgent);
        Task<BaseResponseDTO<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO request);
        Task<BaseResponseDTO<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO request);
        Task<BaseResponseDTO<string>> VerifyOtpAsync(VerifyOtpRequestDTO request);
        Task<BaseResponseDTO<string>> ResetPasswordAsync(ResetPasswordRequestDTO request);

        // ── SSO Authorization Code Flow ────────────────────────────────────────
        /// <summary>
        /// Phase 1: authenticate with credentials, create UserSession + AppSession + AuthCode.
        /// Returns the auth code and full redirect URL.
        /// </summary>
        Task<BaseResponseDTO<SsoAuthorizeResponseDTO>> AuthorizeWithCredentialsAsync(
            SsoLoginRequestDTO request, string? ipAddress, string? userAgent);

        /// <summary>
        /// Phase 2: SSO bypass — look up an existing valid UserSession (from cookie),
        /// create a new AppSession + AuthCode for a different app, no password needed.
        /// </summary>
        Task<BaseResponseDTO<SsoAuthorizeResponseDTO>> AuthorizeWithSessionAsync(
            Guid userSessionId, string appCode, string redirectUri);

        /// <summary>
        /// Exchange an auth code for scoped tokens (server-to-server call from App backend).
        /// Validates: grant_type, client ID, client secret, redirectUri, PKCE.
        /// </summary>
        Task<BaseResponseDTO<LoginResponseDTO>> ExchangeCodeForTokenAsync(SsoTokenRequestDTO request);
    }
}

