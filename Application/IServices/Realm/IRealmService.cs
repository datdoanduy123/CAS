using Application.DTOs.Common;
using Application.DTOs.Realm;

namespace Application.IServices.Realm
{
    public interface IRealmService
    {
        Task<List<RealmListItemDTO>> GetAllAsync(QueryDTO<RealmQueryDTO> model);
        Task<RealmDTO> GetByIdAsync(Guid id);
        Task<RealmDTO> CreateAsync(BaseRequestDTO<RealmUpsertDTO> model);
        Task<RealmDTO> UpdateAsync(Guid id, BaseRequestDTO<RealmUpsertDTO> model);
        Task<RealmDTO> SetActiveAsync(Guid id, BaseRequestDTO<RealmActiveDTO> model);
        Task<bool> DeleteAsync(Guid id, BaseRequestDTO model);
    }
}
