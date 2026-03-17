using Application.DTOs.Common;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepositories.User
{
    public interface IUserRepository
    {
        Task<List<UserListItemDTO>> Search(QueryDTO<UserQueryDTO> model);
    }
}
