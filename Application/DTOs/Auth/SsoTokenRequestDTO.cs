namespace Application.DTOs.Auth
{
    /// <summary>
    /// Request body for POST /api/auth/token.
    /// Called server-to-server by App-A/B backend to exchange an auth code for tokens.
    /// </summary>
    public class SsoTokenRequestDTO
    {
        /// <summary>Must be "authorization_code".</summary>
        public string GrantType { get; set; } = string.Empty;

        /// <summary>The authorization code received from the /login redirect.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Client ID — the App's unique code (e.g. "app-a").</summary>
        public string AppCode { get; set; } = string.Empty;

        /// <summary>Client Secret — proves the caller is the legitimate App (Confidential clients).</summary>
        public string AppSecret { get; set; } = string.Empty;

        /// <summary>Must exactly match the redirectUri that was sent during /authorize.</summary>
        public string RedirectUri { get; set; } = string.Empty;

    }
}
