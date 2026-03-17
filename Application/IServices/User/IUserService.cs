using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices.User
{
    public interface IUserService
    {
        Task<Application.DTOs.Common.BaseResponseDTO<List<Application.DTOs.User.UserListItemDTO>>> GetAllAsync(Application.DTOs.Common.QueryDTO<Application.DTOs.User.UserQueryDTO> model);
    }
}
