using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepositories.User
{
    public interface IUserRepository
    {
        Task<List<Application.DTOs.User.UserListItemDTO>> Search(Application.DTOs.Common.QueryDTO<Application.DTOs.User.UserQueryDTO> model);
    }
}
