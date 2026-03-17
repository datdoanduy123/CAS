using System;

namespace Application.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }

        public Guid UserSessionId { get; set; }
        public Guid AppSessionId { get; set; }
    }
}
