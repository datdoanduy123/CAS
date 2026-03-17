using Application.DTOs.Auth;
using Application.DTOs.Common;
using System.Threading.Tasks;

namespace Application.IServices.Auth
{
    public interface IAuthService
    {
        Task<BaseResponseDTO<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, string appCode, string? ipAddress, string? userAgent);
        Task<BaseResponseDTO<LoginResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, string appCode, string? ipAddress, string? userAgent);
    }
}
