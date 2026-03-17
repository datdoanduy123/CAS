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
        private readonly Application.IRepositories.User.IUserRepository _userRepository;

        public UserService(Application.IRepositories.User.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Application.DTOs.Common.BaseResponseDTO<List<Application.DTOs.User.UserListItemDTO>>> GetAllAsync(Application.DTOs.Common.QueryDTO<Application.DTOs.User.UserQueryDTO> model)
        {
            // Logic phân trang, tìm kiếm và mapping đã được xử lý tập trung trong Repository theo mẫu mới
            var data = await _userRepository.Search(model);

            var metaData = new Application.DTOs.Common.MetaDataDTO
            {
                Page = model.Page,
                PageSize = model.PageSize,
                Total = model.Total
            };

            return Application.DTOs.Common.BaseResponseDTO<List<Application.DTOs.User.UserListItemDTO>>.SuccessResponse(data, metaData);
        }
    }
}
