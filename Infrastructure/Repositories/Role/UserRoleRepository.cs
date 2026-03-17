using Application.DTOs.Role;
using Application.IRepositories.Role;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Role
{
    /// <summary>
    /// Triển khai cụ thể của IUserRoleRepository sử dụng EF Core.
    /// Chứa các thao tác CRUD trực tiếp trên bảng trung gian UserRole.
    /// </summary>
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách Role của 1 User, Join sang bảng Role và App để lấy tên.
        /// Phân biệt rõ Realm Role (AppId null) và App Role (có AppId).
        /// </summary>
        public async Task<List<UserRoleDTO>> GetRolesByUserIdAsync(Guid userId)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r!.App)
                .Select(ur => new UserRoleDTO
                {
                    RoleId      = ur.RoleId,
                    Name        = ur.Role!.Name,
                    Code        = ur.Role!.Code,
                    Description = ur.Role!.Description,
                    AppId       = ur.Role!.AppId,
                    // Nếu AppId null => Realm Role, AppName sẽ là null
                    AppName     = ur.Role!.App != null ? ur.Role!.App.Name : null
                })
                .OrderBy(r => r.AppId == null ? 0 : 1) // Realm Role lên đầu
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Thêm nhiều UserRole cùng lúc (Bulk insert).
        /// Dùng EF Core AddRangeAsync để tối ưu hiệu suất.
        /// </summary>
        public async Task<bool> AddRangeAsync(List<UserRole> userRoles)
        {
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa các UserRole theo UserId và danh sách RoleId cụ thể.
        /// Chỉ xóa đúng những quyền được chỉ định.
        /// </summary>
        public async Task<bool> RemoveRangeAsync(Guid userId, List<Guid> roleIds)
        {
            var records = await _context.UserRoles
                .Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId))
                .ToListAsync();

            if (records.Count == 0) return true; // Không có gì để xóa

            _context.UserRoles.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa toàn bộ Role của 1 User.
        /// Dùng trước khi Sync lại danh sách Role (PUT /roles).
        /// </summary>
        public async Task<bool> RemoveAllByUserIdAsync(Guid userId)
        {
            var records = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (records.Count == 0) return true;

            _context.UserRoles.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lọc danh sách RoleId chỉ giữ lại những ID thực sự tồn tại trong DB.
        /// Tránh lỗi FK khi gán RoleId không hợp lệ.
        /// </summary>
        public async Task<List<Guid>> FilterValidRoleIdsAsync(List<Guid> roleIds)
        {
            return await _context.Roles
                .AsNoTracking()
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();
        }
    }
}
