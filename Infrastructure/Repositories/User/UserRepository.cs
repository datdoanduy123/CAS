using Application.DTOs.Common;
using Application.DTOs.User;
using Application.IRepositories.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserListItemDTO>> Search(QueryDTO<UserQueryDTO> model)
        {
            var query = EntityFrameworkQueryableExtensions.AsNoTracking(_context.Users)
                .Where(x => x.IsActive) // Mặc định chỉ lấy những người đang hoạt động
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();

            // 1. Lọc theo Keyword (Username hoặc FullName)
            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var keyword = model.Keyword.Trim().ToLower();
                query = query.Where(x => x.Username.ToLower().Contains(keyword) || (x.FullName != null && x.FullName.ToLower().Contains(keyword)));
            }

            // 2. Lọc theo các điều kiện bổ sung trong UserQueryDTO
            if (model.Query != null)
            {
                if (model.Query.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == model.Query.IsActive.Value);
                }

                if (model.Query.RealmId.HasValue)
                {
                    query = query.Where(x => x.RealmId == model.Query.RealmId.Value);
                }
            }

            // 3. Tính tổng số bản ghi
            model.Total = await EntityFrameworkQueryableExtensions.CountAsync(query);

            // 4. Phân trang
            if (!model.IsGetAll)
            {
                query = query.Skip(model.Skip).Take(model.PageSize);
            }

            // 5. Mapping trực tiếp sang DTO
            return (await EntityFrameworkQueryableExtensions.ToListAsync(query))
                .Select(x => new Application.DTOs.User.UserListItemDTO
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    FullName = x.FullName,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task<Domain.Entities.User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<Domain.Entities.User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Domain.Entities.User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> CapNhat(Domain.Entities.User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> XoaCung(Domain.Entities.User user)
        {
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TaoMoi(CreateUserDTO dto)
        {
            var entity = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                PasswordHash = dto.Password, // TODO: Hash password
                Email = dto.Email,
                FullName = dto.FullName,
                RealmId = dto.RealmId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Users.AddAsync(entity);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
