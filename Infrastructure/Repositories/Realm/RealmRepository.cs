using Application.DTOs.Common;
using Application.DTOs.Realm;
using Application.IRepositories.Realm;
using Domain.Constants;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RealmEntity = Domain.Entities.Realm;

namespace Infrastructure.Repositories.Realm
{
    public class RealmRepository : IRealmRepository
    {
        private readonly AppDbContext _context;

        public RealmRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RealmListItemDTO>> SearchAsync(QueryDTO<RealmQueryDTO> model)
        {
            var query = _context.Realms
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .AsQueryable();

            // Default: chỉ lấy active, trừ khi client truyền isActive cụ thể hoặc includeInactive=true
            if (model.Query?.IsActive.HasValue == true)
            {
                query = query.Where(x => x.IsActive == model.Query.IsActive.Value);
            }
            else if (model.Query?.IncludeInactive != true)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var keyword = model.Keyword.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(keyword) ||
                    (x.DisplayName != null && x.DisplayName.ToLower().Contains(keyword)));
            }

            model.Total = await query.CountAsync();

            if (!model.IsGetAll)
            {
                query = query.Skip(model.Skip).Take(model.PageSize);
            }

            var realms = await query.ToListAsync();
            return realms.Select(MapToListItemDto).ToList();
        }

        public async Task<RealmDTO> GetByIdAsync(Guid id)
        {
            var realm = await _context.Realms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (realm == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            return MapToDto(realm);
        }

        public async Task<RealmDTO> CreateAsync(BaseRequestDTO<RealmUpsertDTO> model)
        {
            ValidateRequest(model.Request);
            await EnsureUniqueNameAsync(model.Request.Name);

            var realm = new RealmEntity
            {
                Name = model.Request.Name.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(model.Request.DisplayName) ? null : model.Request.DisplayName.Trim(),
                IsActive = true
            };

            await _context.Realms.AddAsync(realm);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(realm.Id);
        }

        public async Task<RealmDTO> UpdateAsync(Guid id, BaseRequestDTO<RealmUpsertDTO> model)
        {
            ValidateRequest(model.Request);

            var realm = await _context.Realms.FirstOrDefaultAsync(x => x.Id == id);
            if (realm == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            await EnsureUniqueNameAsync(model.Request.Name, id);

            realm.Name = model.Request.Name.Trim();
            realm.DisplayName = string.IsNullOrWhiteSpace(model.Request.DisplayName) ? null : model.Request.DisplayName.Trim();

            await _context.SaveChangesAsync();

            return await GetByIdAsync(realm.Id);
        }

        public async Task<RealmDTO> SetActiveAsync(Guid id, BaseRequestDTO<RealmActiveDTO> model)
        {
            if (model?.Request == null)
            {
                throw new ApplicationException(MessageConstant.CommonMessage.MISSING_PARAM);
            }

            var realm = await _context.Realms.FirstOrDefaultAsync(x => x.Id == id);
            if (realm == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            realm.IsActive = model.Request.IsActive;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(realm.Id);
        }

        public async Task<bool> DeleteAsync(Guid id, BaseRequestDTO model)
        {
            var realm = await _context.Realms.FirstOrDefaultAsync(x => x.Id == id);
            if (realm == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            _context.Realms.Remove(realm);
            await _context.SaveChangesAsync();

            return true;
        }

        private static void ValidateRequest(RealmUpsertDTO request)
        {
            if (request == null)
            {
                throw new ApplicationException(MessageConstant.CommonMessage.MISSING_PARAM);
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ApplicationException(MessageConstant.CommonMessage.MISSING_PARAM);
            }
        }

        private async Task EnsureUniqueNameAsync(string name, Guid? excludeId = null)
        {
            var normalizedName = name.Trim().ToUpper();

            var duplicateExists = await _context.Realms.AnyAsync(x =>
                (!excludeId.HasValue || x.Id != excludeId.Value) &&
                x.Name.ToUpper() == normalizedName);

            if (duplicateExists)
            {
                throw new ApplicationException(MessageConstant.CommonMessage.FAILED);
            }
        }

        private static RealmListItemDTO MapToListItemDto(RealmEntity realm)
        {
            return new RealmListItemDTO
            {
                Id = realm.Id,
                Name = realm.Name,
                DisplayName = realm.DisplayName,
                IsActive = realm.IsActive
            };
        }

        private static RealmDTO MapToDto(RealmEntity realm)
        {
            return new RealmDTO
            {
                Id = realm.Id,
                Name = realm.Name,
                DisplayName = realm.DisplayName,
                IsActive = realm.IsActive
            };
        }
    }
}
