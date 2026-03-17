using Application.DTOs.Common;
using Application.DTOs.Realm;

namespace Application.IRepositories.Realm
{
    public interface IRealmRepository
    {
        Task<List<RealmListItemDTO>> SearchAsync(QueryDTO<RealmQueryDTO> model);
        Task<RealmDTO> GetByIdAsync(Guid id);
        Task<RealmDTO> CreateAsync(BaseRequestDTO<RealmUpsertDTO> model);
        Task<RealmDTO> UpdateAsync(Guid id, BaseRequestDTO<RealmUpsertDTO> model);
        Task<RealmDTO> SetActiveAsync(Guid id, BaseRequestDTO<RealmActiveDTO> model);
        Task<bool> DeleteAsync(Guid id, BaseRequestDTO model);
    }
}
