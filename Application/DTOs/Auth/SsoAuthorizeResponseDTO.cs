using System;

namespace Application.DTOs.Auth
{
    public class SsoAuthorizeResponseDTO
    {
        /// <summary>
        /// The short-lived authorization code to be sent back to the App via redirect.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Full redirect URL including the code: redirectUri?code=CODE
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// The UserSession ID created during Phase 1 login.
        /// Used by the AuthController to set the HttpOnly SSO cookie.
        /// Null on Phase 2 SSO (session already exists).
        /// </summary>
        public Guid? UserSessionId { get; set; }
    }
}

