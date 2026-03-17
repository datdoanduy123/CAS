using Application.DTOs.Common;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IRepositories.User
{
    public interface IUserRepository
    {
        Task<List<UserListItemDTO>> Search(QueryDTO<UserQueryDTO> model);
        Task<Domain.Entities.User?> GetByUsernameAsync(string username);
        Task<Domain.Entities.User?> GetByEmailAsync(string email);
        Task<Domain.Entities.User?> GetByIdAsync(Guid id);
        Task<bool> CapNhat(Domain.Entities.User user);
        Task<bool> XoaCung(Domain.Entities.User user);
        Task<bool> TaoMoi(CreateUserDTO request);
    }
}
