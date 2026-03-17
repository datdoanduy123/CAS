using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class VerifyOtpRequestDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 ký tự.")]
        public string Otp { get; set; } = string.Empty;
    }
}
