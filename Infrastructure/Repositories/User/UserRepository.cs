using Application.IRepositories.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly Infrastructure.Persistence.AppDbContext _context;

        public UserRepository(Infrastructure.Persistence.AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Application.DTOs.User.UserListItemDTO>> Search(Application.DTOs.Common.QueryDTO<Application.DTOs.User.UserQueryDTO> model)
        {
            var query = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_context.Users)
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
            model.Total = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);

            // 4. Phân trang
            if (!model.IsGetAll)
            {
                query = query.Skip(model.Skip).Take(model.PageSize);
            }

            // 5. Mapping trực tiếp sang DTO
            return (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query))
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
    }
}
