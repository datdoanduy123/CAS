using Application.DTOs.Auth;
using Application.DTOs.Common;
using System.Threading.Tasks;

namespace Application.IServices.Auth
{
    public interface IAuthService
    {
        Task<BaseResponseDTO<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, string appCode, string? ipAddress, string? userAgent);
        Task<BaseResponseDTO<LoginResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, string appCode, string? ipAddress, string? userAgent);
        Task<BaseResponseDTO<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO request);
        Task<BaseResponseDTO<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO request);
        Task<BaseResponseDTO<string>> VerifyOtpAsync(VerifyOtpRequestDTO request);
        Task<BaseResponseDTO<string>> ResetPasswordAsync(ResetPasswordRequestDTO request);
    }
}
