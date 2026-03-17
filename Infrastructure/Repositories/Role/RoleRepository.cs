using Application.DTOs.Common;
using Application.DTOs.Role;
using Application.IRepositories.Role;
using Domain.Constants;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RoleEntity = Domain.Entities.Role;

namespace Infrastructure.Repositories.Role
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleListItemDTO>> SearchAsync(QueryDTO<RoleQueryDTO> model)
        {
            var query = _context.Roles
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var keyword = model.Keyword.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(keyword) ||
                    x.Code.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            model.Total = await query.CountAsync();

            if (!model.IsGetAll)
            {
                query = query.Skip(model.Skip).Take(model.PageSize);
            }

            var roles = await query.ToListAsync();
            return roles.Select(MapToListItemDto).ToList();
        }

        public async Task<RoleDTO> GetByIdAsync(Guid id)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (role == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            return MapToDto(role);
        }

        public async Task<RoleDTO> CreateAsync(BaseRequestDTO<RoleUpsertDTO> model)
        {
            ValidateRequest(model.Request);
            await EnsureUniqueAsync(model.Request.Name, model.Request.Code);

            var role = new RoleEntity
            {
                Name = model.Request.Name.Trim(),
                Code = model.Request.Code.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Request.Description) ? null : model.Request.Description.Trim()
            };

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(role.Id);
        }

        public async Task<RoleDTO> UpdateAsync(Guid id, BaseRequestDTO<RoleUpsertDTO> model)
        {
            ValidateRequest(model.Request);

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            if (role == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            await EnsureUniqueAsync(model.Request.Name, model.Request.Code, id);

            role.Name = model.Request.Name.Trim();
            role.Code = model.Request.Code.Trim();
            role.Description = string.IsNullOrWhiteSpace(model.Request.Description) ? null : model.Request.Description.Trim();

            await _context.SaveChangesAsync();

            return await GetByIdAsync(role.Id);
        }

        public async Task<bool> DeleteAsync(Guid id, BaseRequestDTO model)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            if (role == null)
            {
                throw new KeyNotFoundException(MessageConstant.CommonMessage.NOT_FOUND);
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return true;
        }

        private static void ValidateRequest(RoleUpsertDTO request)
        {
            if (request == null)
            {
                throw new ApplicationException(MessageConstant.CommonMessage.MISSING_PARAM);
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
            {
                throw new ApplicationException(MessageConstant.CommonMessage.MISSING_PARAM);
            }
        }

        private async Task EnsureUniqueAsync(string name, string code, Guid? excludeId = null)
        {
            var normalizedName = name.Trim().ToUpper();
            var normalizedCode = code.Trim().ToUpper();

            var duplicateExists = await _context.Roles.AnyAsync(x =>
                (!excludeId.HasValue || x.Id != excludeId.Value) &&
                (x.Name.ToUpper() == normalizedName || x.Code.ToUpper() == normalizedCode));

            if (duplicateExists)
            {
                throw new ApplicationException(MessageConstant.CommonMessage.FAILED);
            }
        }

        private static RoleListItemDTO MapToListItemDto(RoleEntity role)
        {
            return new RoleListItemDTO
            {
                Id = role.Id,
                Name = role.Name,
                Code = role.Code,
                Description = role.Description
            };
        }

        private static RoleDTO MapToDto(RoleEntity role)
        {
            return new RoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                Code = role.Code,
                Description = role.Description
            };
        }
    }
}
