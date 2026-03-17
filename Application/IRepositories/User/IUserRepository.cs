using System.Threading.Tasks;
using UserEntity = Domain.Entities.User;

namespace Application.IRepositories.User
{
    public interface IUserRepository
    {
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task<UserEntity> CreateAsync(UserEntity user);
    }
}
