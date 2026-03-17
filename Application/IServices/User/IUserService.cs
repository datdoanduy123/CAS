using Application.DTOs.Common;
using Application.DTOs.User;
using System.Threading.Tasks;

namespace Application.IServices.User
{
    public interface IUserService
    {
        Task<BaseResponseDTO<CreateUserResponseDTO>> CreateUserAsync(CreateUserRequestDTO request);
    }
}
