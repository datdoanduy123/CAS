using Application.DTOs.Common;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices.User
{
    public interface IUserService
    {
        Task<List<UserListItemDTO>> GetAllAsync(QueryDTO<UserQueryDTO> model);
        Task<bool> CreateAsync(CreateUserDTO request);
        Task<bool> UpdateAsync(Guid id, UpdateUserDTO request);
        Task<UserListItemDTO?> GetByIdAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
