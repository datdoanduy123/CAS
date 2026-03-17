using Application.DTOs.App;
using Application.DTOs.Common;
using Application.IRepositories.App;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.App
{
    public class AppRepository : IAppRepository
    {
        private readonly AppDbContext _context;

        public AppRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppListItemDTO>> Search(QueryDTO<AppQueryDTO> model)
        {
            var query = _context.Apps.AsNoTracking().AsQueryable();

            // 1. Tìm theo keyword (Name hoặc Code)
            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var keyword = model.Keyword.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            // 2. Lọc theo QueryDTO
            if (model.Query != null)
            {
                if (model.Query.RealmId.HasValue)
                {
                    query = query.Where(x => x.RealmId == model.Query.RealmId.Value);
                }

                if (model.Query.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == model.Query.IsActive.Value);
                }
            }

            // 3. Tính tổng
            model.Total = await query.CountAsync();

            // 4. Phân trang
            if (!model.IsGetAll)
            {
                query = query.OrderBy(x => x.Name).Skip(model.Skip).Take(model.PageSize);
            }

            // 5. Mapping
            return await query.Select(x => new AppListItemDTO
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                AppType = x.AppType,
                IsActive = x.IsActive,
                RealmId = x.RealmId
            }).ToListAsync();
        }

        public async Task<Domain.Entities.App?> GetByIdAsync(Guid id)
        {
            return await _context.Apps.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> CapNhat(Domain.Entities.App app)
        {
            _context.Apps.Update(app);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> XoaCung(Domain.Entities.App app)
        {
            _context.Apps.Remove(app);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Domain.Entities.App?> GetByCodeAsync(string code)
        {
            return await _context.Apps.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<bool> TaoMoi(CreateAppDTO dto, string hashedSecret)
        {
            var entity = new Domain.Entities.App
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code.Trim(),
                AppSecret = hashedSecret,
                RedirectUris = dto.RedirectUris,
                AllowedGrantTypes = dto.AllowedGrantTypes,
                LogoutUri = dto.LogoutUri,
                AppType = dto.AppType,
                RealmId = dto.RealmId,
                IsActive = true
            };

            await _context.Apps.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
