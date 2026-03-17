using Application.DTOs.App;
using Application.DTOs.Common;
using Application.IRepositories.App;
using Application.IServices.App;
using Domain.Constants;
using System;
using System.Threading.Tasks;

namespace Application.Services.App
{
    public class AppService : IAppService
    {
        private readonly IAppRepository _appRepository;

        public AppService(IAppRepository appRepository)
        {
            _appRepository = appRepository;
        }

        public async Task<List<AppListItemDTO>> GetAllAsync(QueryDTO<AppQueryDTO> model)
        {
            return await _appRepository.Search(model);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var app = await _appRepository.GetByIdAsync(id);
            if (app == null) throw new ApplicationException(Domain.Constants.MessageConstant.AppMessage.APP_NOT_FOUND);

            return await _appRepository.XoaCung(app);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var app = await _appRepository.GetByIdAsync(id);
            if (app == null) throw new ApplicationException(Domain.Constants.MessageConstant.AppMessage.APP_NOT_FOUND);

            app.IsActive = false;
            return await _appRepository.CapNhat(app);
        }

        public async Task<AppListItemDTO?> GetByIdAsync(Guid id)
        {
            var app = await _appRepository.GetByIdAsync(id);
            if (app == null) return null;

            return new AppListItemDTO
            {
                Id = app.Id,
                Name = app.Name,
                Code = app.Code,
                AppType = app.AppType,
                IsActive = app.IsActive,
                RealmId = app.RealmId
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateAppDTO request)
        {
            // 1. Tìm app
            var app = await _appRepository.GetByIdAsync(id);
            if (app == null) throw new ApplicationException(Domain.Constants.MessageConstant.AppMessage.APP_NOT_FOUND);

            // 2. Cập nhật các trường
            if (!string.IsNullOrEmpty(request.Name)) app.Name = request.Name;
            if (!string.IsNullOrEmpty(request.AppSecret)) app.AppSecret = request.AppSecret;
            if (!string.IsNullOrEmpty(request.RedirectUris)) app.RedirectUris = request.RedirectUris;
            if (!string.IsNullOrEmpty(request.AllowedGrantTypes)) app.AllowedGrantTypes = request.AllowedGrantTypes;
            if (!string.IsNullOrEmpty(request.LogoutUri)) app.LogoutUri = request.LogoutUri;
            if (!string.IsNullOrEmpty(request.AppType)) app.AppType = request.AppType;
            if (request.IsActive.HasValue) app.IsActive = request.IsActive.Value;
            if (request.RealmId.HasValue) app.RealmId = request.RealmId;

            // 3. Lưu
            return await _appRepository.CapNhat(app);
        }

        public async Task<bool> CreateAsync(CreateAppDTO request)
        {
            // 1. Kiểm tra tồn tại qua AppCode
            var existingApp = await _appRepository.GetByCodeAsync(request.Code);
            if (existingApp != null) throw new ApplicationException(MessageConstant.AppMessage.APP_EXIST);

            // 2. Repo xử lý tạo mới
            return await _appRepository.TaoMoi(request);
        }
    }
}
