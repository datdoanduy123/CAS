using Application.IRepositories.Auth;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.App?> GetAppByCodeAsync(string appCode)
        {
            return await _context.Apps.FirstOrDefaultAsync(a => a.Code == appCode && a.IsActive);
        }

        public async Task SaveSessionsAsync(UserSession userSession, AppSession appSession)
        {
            await _context.UserSessions.AddAsync(userSession);
            await _context.AppSessions.AddAsync(appSession);
            await _context.SaveChangesAsync();
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenValue)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                    .ThenInclude(u => u.UserRoles!)
                        .ThenInclude(ur => ur.Role)
                .Include(r => r.App)
                .FirstOrDefaultAsync(r => r.TokenValue == tokenValue);
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserLastLoginAsync(Domain.Entities.User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserPasswordAsync(Domain.Entities.User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
