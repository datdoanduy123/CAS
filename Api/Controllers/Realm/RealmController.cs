using Application.DTOs.Common;
using Application.DTOs.Realm;
using Application.IServices.Realm;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Realm
{
    [Route("api/[controller]")]
    [ApiController]
    public class RealmController : BaseController
    {
        private readonly IRealmService _realmService;

        public RealmController(IRealmService realmService)
        {
            _realmService = realmService;
        }

        [HttpGet]
        public async Task<BaseResponseDTO<List<RealmListItemDTO>>> GetAll([FromQuery] BaseQueryDTO request, [FromQuery] RealmQueryDTO filter)
        {
            var query = new QueryDTO<RealmQueryDTO>
            {
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Query = filter,
                ActionBy = UserId,
                IsAdmin = IsAdmin,
                Roles = Roles
            };

            var dataTask = _realmService.GetAllAsync(query);
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
        public async Task<BaseResponseDTO<RealmDTO>> GetDetail(Guid id)
        {
            return await HandleException(_realmService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<BaseResponseDTO<RealmDTO>> Create([FromBody] RealmUpsertDTO request)
        {
            var model = new BaseRequestDTO<RealmUpsertDTO>
            {
                Request = request,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_realmService.CreateAsync(model));
        }

        [HttpPut("{id:guid}")]
        public async Task<BaseResponseDTO<RealmDTO>> Update(Guid id, [FromBody] RealmUpsertDTO request)
        {
            var model = new BaseRequestDTO<RealmUpsertDTO>
            {
                Request = request,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_realmService.UpdateAsync(id, model));
        }

        [HttpPatch("{id:guid}/active")]
        public async Task<BaseResponseDTO<RealmDTO>> SetActive(Guid id, [FromBody] RealmActiveDTO request)
        {
            var model = new BaseRequestDTO<RealmActiveDTO>
            {
                Request = request,
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_realmService.SetActiveAsync(id, model));
        }

        [HttpDelete("{id:guid}")]
        public async Task<BaseResponseDTO<bool>> Delete(Guid id)
        {
            var model = new BaseRequestDTO
            {
                ActionBy = UserId,
                IsAdmin = IsAdmin
            };

            return await HandleException(_realmService.DeleteAsync(id, model));
        }
    }
}
