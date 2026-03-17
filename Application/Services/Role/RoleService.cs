using Application.DTOs.Common;
using Application.DTOs.Role;
using Application.IRepositories.Role;
using Application.IServices.Role;

namespace Application.Services.Role
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<RoleListItemDTO>> GetAllAsync(QueryDTO<RoleQueryDTO> model)
        {
            return await _roleRepository.SearchAsync(model);
        }

        public async Task<RoleDTO> GetByIdAsync(Guid id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        public async Task<RoleDTO> CreateAsync(BaseRequestDTO<RoleUpsertDTO> model)
        {
            return await _roleRepository.CreateAsync(model);
        }

        public async Task<RoleDTO> UpdateAsync(Guid id, BaseRequestDTO<RoleUpsertDTO> model)
        {
            return await _roleRepository.UpdateAsync(id, model);
        }

        public async Task<RoleDTO> SetActiveAsync(Guid id, BaseRequestDTO<RoleActiveDTO> model)
        {
            return await _roleRepository.SetActiveAsync(id, model);
        }

        public async Task<bool> DeleteAsync(Guid id, BaseRequestDTO model)
        {
            return await _roleRepository.DeleteAsync(id, model);
        }
    }
}