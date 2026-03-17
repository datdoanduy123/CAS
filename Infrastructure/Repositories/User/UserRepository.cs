using Application.IRepositories.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UserEntity = Domain.Entities.User;

namespace Infrastructure.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<UserEntity> CreateAsync(UserEntity user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}

