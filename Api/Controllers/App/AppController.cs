using Application.DTOs.App;
using Application.DTOs.Common;
using Application.IServices.App;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers.App
{
    [ApiController]
    [Route("api/app")]
    public class AppController : BaseController
    {
        private readonly IAppService _appService;

        public AppController(IAppService appService)
        {
            _appService = appService;
        }

        [HttpGet("{id}")]
        public async Task<BaseResponseDTO<AppListItemDTO?>> GetById([FromRoute] Guid id)
        {
            return await HandleException(_appService.GetByIdAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<BaseResponseDTO<bool>> Delete([FromRoute] Guid id)
        {
            return await HandleException(_appService.DeleteAsync(id));
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<BaseResponseDTO<bool>> SoftDelete([FromRoute] Guid id)
        {
            return await HandleException(_appService.SoftDeleteAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<BaseResponseDTO<bool>> Update([FromRoute] Guid id, [FromBody] UpdateAppDTO request)
        {
            return await HandleException(_appService.UpdateAsync(id, request));
        }

        [HttpPost]
        public async Task<BaseResponseDTO<bool>> Create([FromBody] CreateAppDTO request)
        {
            return await HandleException(_appService.CreateAsync(request));
        }

        [HttpGet]
        public async Task<BaseResponseDTO<List<AppListItemDTO>>> GetAll([FromQuery] BaseQueryDTO baseQuery, [FromQuery] AppQueryDTO appQuery)
        {
            var model = new QueryDTO<AppQueryDTO>
            {
                Keyword = baseQuery.Keyword,
                Page = baseQuery.Page,
                PageSize = baseQuery.PageSize,
                Query = appQuery
            };

            var data = await _appService.GetAllAsync(model);

            return BaseResponseDTO<List<AppListItemDTO>>.SuccessResponse(data, new MetaDataDTO
            {
                Total = model.Total,
                Page = model.Page,
                PageSize = model.PageSize
            });
        }
    }
}
