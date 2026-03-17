using Application.DTOs.Common;
using Application.DTOs.User;
using Application.IRepositories.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var keyword = model.Keyword.Trim().ToLower();
                query = query.Where(x => x.Username.ToLower().Contains(keyword) || (x.FullName != null && x.FullName.ToLower().Contains(keyword)));
            }

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

            model.Total = await query.CountAsync();

            if (!model.IsGetAll)
            {
                query = query.OrderByDescending(x => x.CreatedAt).Skip(model.Skip).Take(model.PageSize);
            }

            return await query.Select(x => new UserListItemDTO
            {
                Id = x.Id,
                Username = x.Username,
                Email = x.Email,
                FullName = x.FullName,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            }).ToListAsync();
        }

        public async Task<Domain.Entities.User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
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

        public async Task<bool> TaoMoi(CreateUserDTO dto, string hashedPassword, string salt)
        {
            var entity = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
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
