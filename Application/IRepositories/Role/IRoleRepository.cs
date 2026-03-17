using Application.DTOs.Common;
using Application.DTOs.Role;

namespace Application.IRepositories.Role
{
    public interface IRoleRepository
    {
        Task<List<RoleListItemDTO>> SearchAsync(QueryDTO<RoleQueryDTO> model);
        Task<RoleDTO> GetByIdAsync(Guid id);
        Task<RoleDTO> CreateAsync(BaseRequestDTO<RoleUpsertDTO> model);
        Task<RoleDTO> UpdateAsync(Guid id, BaseRequestDTO<RoleUpsertDTO> model);
        Task<RoleDTO> SetActiveAsync(Guid id, BaseRequestDTO<RoleActiveDTO> model);
        Task<bool> DeleteAsync(Guid id, BaseRequestDTO model);
    }
}