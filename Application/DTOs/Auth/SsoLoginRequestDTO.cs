namespace Application.DTOs.Auth
{
    /// <summary>
    /// Request body for POST /api/auth/login.
    /// User submits their credentials along with the SSO context params.
    /// </summary>
    public class SsoLoginRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>Which app the user is logging in to.</summary>
        public string AppCode { get; set; } = string.Empty;

        /// <summary>Must be a registered redirect URI for the App.</summary>
        public string RedirectUri { get; set; } = string.Empty;

    }
}
