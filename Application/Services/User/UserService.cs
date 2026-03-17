using Application.DTOs.Common;
using Application.DTOs.User;
using Application.IRepositories.User;
using Application.IServices.User;
using System;
using System.Threading.Tasks;
using UserEntity = Domain.Entities.User;
using static Domain.Constants.MessageConstant;

namespace Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponseDTO<CreateUserResponseDTO>> CreateUserAsync(CreateUserRequestDTO request)
        {
            // Kiểm tra username trùng
            if (await _userRepository.IsUsernameExistsAsync(request.Username))
                return BaseResponseDTO<CreateUserResponseDTO>.FailResponse(UserMessage.USERNAME_EXISTED, 400);

            // Kiểm tra email trùng
            if (await _userRepository.IsEmailExistsAsync(request.Email))
                return BaseResponseDTO<CreateUserResponseDTO>.FailResponse(UserMessage.EMAIL_EXISTED, 400);

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo entity
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLower(),
                PasswordHash = passwordHash,
                FullName = request.FullName?.Trim(),
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                RealmId = request.RealmId
            };

            var created = await _userRepository.CreateAsync(user);

            var response = new CreateUserResponseDTO
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                FullName = created.FullName,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedAt,
                RealmId = created.RealmId
            };

            return BaseResponseDTO<CreateUserResponseDTO>.SuccessResponse(response, UserMessage.CREATE_SUCCESS, 201);
        }
    }
}

