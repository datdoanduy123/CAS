using Application.DTOs.Common;
using Application.DTOs.User;
using Application.IRepositories.User;
using Application.IServices.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserListItemDTO>> GetAllAsync(QueryDTO<UserQueryDTO> model)
        {
            return await _userRepository.Search(model);
        }
    }
}
