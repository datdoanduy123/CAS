using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class ChangePasswordRequestDTO
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
