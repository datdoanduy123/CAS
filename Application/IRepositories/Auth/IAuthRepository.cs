using Domain.Entities;
using System.Threading.Tasks;

namespace Application.IRepositories.Auth
{
    public interface IAuthRepository
    {
        Task<Domain.Entities.App?> GetAppByCodeAsync(string appCode);
        Task SaveSessionsAsync(UserSession userSession, AppSession appSession);
        Task UpdateUserLastLoginAsync(Domain.Entities.User user);
    }
}
