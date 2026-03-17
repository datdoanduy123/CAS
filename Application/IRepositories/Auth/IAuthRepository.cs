using Domain.Entities;
using System.Threading.Tasks;

namespace Application.IRepositories.Auth
{
    public interface IAuthRepository
    {
        Task<Domain.Entities.App?> GetAppByCodeAsync(string appCode);
        Task SaveSessionsAsync(UserSession userSession, AppSession appSession);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string tokenValue);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateUserLastLoginAsync(Domain.Entities.User user);
    }
}
