using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class ForgotPasswordRequestDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;
    }
}
