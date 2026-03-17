using Application.DTOs.Common;
using Application.DTOs.Realm;
using Application.IRepositories.Realm;
using Application.IServices.Realm;

namespace Application.Services.Realm
{
    public class RealmService : IRealmService
    {
        private readonly IRealmRepository _realmRepository;

        public RealmService(IRealmRepository realmRepository)
        {
            _realmRepository = realmRepository;
        }

        public async Task<List<RealmListItemDTO>> GetAllAsync(QueryDTO<RealmQueryDTO> model)
        {
            return await _realmRepository.SearchAsync(model);
        }

        public async Task<RealmDTO> GetByIdAsync(Guid id)
        {
            return await _realmRepository.GetByIdAsync(id);
        }

        public async Task<RealmDTO> CreateAsync(BaseRequestDTO<RealmUpsertDTO> model)
        {
            return await _realmRepository.CreateAsync(model);
        }

        public async Task<RealmDTO> UpdateAsync(Guid id, BaseRequestDTO<RealmUpsertDTO> model)
        {
            return await _realmRepository.UpdateAsync(id, model);
        }

        public async Task<RealmDTO> SetActiveAsync(Guid id, BaseRequestDTO<RealmActiveDTO> model)
        {
            return await _realmRepository.SetActiveAsync(id, model);
        }

        public async Task<bool> DeleteAsync(Guid id, BaseRequestDTO model)
        {
            return await _realmRepository.DeleteAsync(id, model);
        }
    }
}
