using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class RefreshTokenRequestDTO
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
