using Application.DTOs.App;
using Application.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IServices.App
{
    public interface IAppService
    {
        Task<List<AppListItemDTO>> GetAllAsync(QueryDTO<AppQueryDTO> model);
        Task<AppListItemDTO?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(CreateAppDTO request);
        Task<bool> UpdateAsync(Guid id, UpdateAppDTO request);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
