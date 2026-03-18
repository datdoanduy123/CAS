using Domain.Entities;
using System;
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
        Task UpdateUserPasswordAsync(Domain.Entities.User user);

        // SSO Authorization Code Flow
        Task SaveAuthorizationCodeAsync(AuthorizationCode authCode);
        Task<AuthorizationCode?> GetAuthorizationCodeAsync(string code);
        Task UpdateAuthorizationCodeAsync(AuthorizationCode authCode);
        Task<UserSession?> GetActiveUserSessionAsync(Guid userSessionId);
        Task<Domain.Entities.User?> GetUserWithRolesAsync(Guid userId);
        Task SaveAppSessionAsync(AppSession appSession);
        Task UpdateUserSessionAsync(UserSession userSession);
    }
}
