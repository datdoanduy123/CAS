using Application.DTOs.Common;
using Application.DTOs.Role;
using Application.IServices.Role;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Role
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<BaseResponseDTO<List<RoleListItemDTO>>> GetAll([FromQuery] BaseQueryDTO request, [FromQuery] RoleQueryDTO filter)
        {
            var query = new QueryDTO<RoleQueryDTO>
            {
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Query = filter,
                ActionBy = UserId,
                IsAdmin = IsAdmin,
                Roles = Roles
            };

            var dataTask = _roleService.GetAllAsync(query);
            await dataTask;

            var metaData = new MetaDataDTO
            {
                Page = query.Page,
                PageSize = query.PageSize,
                Total = query.Total
            };

            return await HandleException(dataTask, metaData);
        }

        [HttpGet("{id:guid}")]
        public async Task<BaseResponseDTO<RoleDTO>> GetDetail(Guid id)
        {
            return await HandleException(_roleService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<BaseResponseDTO<RoleDTO>> Create([FromBody] RoleUpsertDTO request)
        {
            var model = new BaseRequestDTO<RoleUpsertDTO>
            {
                Request = request,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_roleService.CreateAsync(model));
        }

        [HttpPut("{id:guid}")]
        public async Task<BaseResponseDTO<RoleDTO>> Update(Guid id, [FromBody] RoleUpsertDTO request)
        {
            var model = new BaseRequestDTO<RoleUpsertDTO>
            {
                Request = request,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_roleService.UpdateAsync(id, model));
        }

        [HttpPatch("{id:guid}/active")]
        public async Task<BaseResponseDTO<RoleDTO>> SetActive(Guid id, [FromBody] RoleActiveDTO request)
        {
            var model = new BaseRequestDTO<RoleActiveDTO>
            {
                Request = request,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_roleService.SetActiveAsync(id, model));
        }

        [HttpDelete("{id:guid}")]
        public async Task<BaseResponseDTO<bool>> Delete(Guid id)
        {
            var model = new BaseRequestDTO
            {
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_roleService.DeleteAsync(id, model));
        }
    }
}
