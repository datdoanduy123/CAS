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

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            return await _userRepository.XoaCung(user);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            user.IsActive = false;
            return await _userRepository.CapNhat(user);
        }

        public async Task<UserListItemDTO?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserListItemDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUserDTO request)
        {
            // 1. Tìm user
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_NOT_FOUND);

            // 2. Kiểm tra email nếu có thay đổi
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (existingEmail != null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.EMAIL_EXIST);
                user.Email = request.Email;
            }

            // 3. Cập nhật các trường khác
            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
            if (request.RealmId.HasValue) user.RealmId = request.RealmId.Value;

            // 4. Lưu
            return await _userRepository.CapNhat(user);
        }

        public async Task<bool> CreateAsync(CreateUserDTO request)
        {
            // 1. Kiểm tra tồn tại
            var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUsername != null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.USER_EXIST);

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null) throw new ApplicationException(Domain.Constants.MessageConstant.UserMessage.EMAIL_EXIST);

            // 2. Repo xử lý tạo mới
            return await _userRepository.TaoMoi(request);
        }

        public async Task<List<UserListItemDTO>> GetAllAsync(QueryDTO<UserQueryDTO> model)
        {
            return await _userRepository.Search(model);
        }
    }
}
