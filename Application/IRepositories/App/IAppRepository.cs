using Application.DTOs.Common;
using Application.DTOs.App;

namespace Application.IRepositories.App
{
    public interface IAppRepository
    {
        Task<List<AppListItemDTO>> Search(QueryDTO<AppQueryDTO> model);
        Task<Domain.Entities.App?> GetByIdAsync(Guid id);
        Task<bool> CapNhat(Domain.Entities.App app);
        Task<bool> XoaCung(Domain.Entities.App app);
        Task<bool> TaoMoi(CreateAppDTO request);
        Task<Domain.Entities.App?> GetByCodeAsync(string code);
    }
}
