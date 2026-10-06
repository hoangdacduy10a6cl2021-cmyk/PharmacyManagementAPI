using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync(string? search, string? role);
        Task<UserDto?> GetByIdAsync(int id);
        Task<(UserDto? data, string? error)> CreateAsync(CreateUserDto dto);
        Task<(UserDto? data, string? error)> UpdateAsync(int id, UpdateUserDto dto, int currentUserId);
        Task<(UserDto? data, string? error)> ToggleActiveAsync(int id, int currentUserId);
        Task<(bool success, string? error)> ResetPasswordAsync(int id, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static bool IsValidRole(string role) => role == "Admin" || role == "NhanVien";

        private IQueryable<UserDto> ProjectToDto(IQueryable<User> query) =>
            query.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                OrderCount = u.Orders.Count
            });

        public async Task<List<UserDto>> GetAllAsync(string? search, string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(s)
                    || u.FullName.ToLower().Contains(s)
                    || (u.Email != null && u.Email.ToLower().Contains(s))
                    || (u.Phone != null && u.Phone.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            return await ProjectToDto(query.OrderBy(u => u.Role).ThenBy(u => u.FullName)).ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            return await ProjectToDto(_context.Users.Where(u => u.Id == id)).FirstOrDefaultAsync();
        }

        public async Task<(UserDto? data, string? error)> CreateAsync(CreateUserDto dto)
        {
            if (!IsValidRole(dto.Role)) return (null, "Vai trò không hợp lệ.");

            var username = dto.Username.Trim();
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return (null, "Tên đăng nhập đã tồn tại.");

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
                Role = dto.Role,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(user.Id), null);
        }

        // Đếm số Admin đang hoạt động ngoài tài khoản này
        private async Task<bool> IsLastActiveAdminAsync(User user)
        {
            if (user.Role != "Admin" || !user.IsActive) return false;
            var others = await _context.Users.CountAsync(u => u.Id != user.Id && u.Role == "Admin" && u.IsActive);
            return others == 0;
        }

        public async Task<(UserDto? data, string? error)> UpdateAsync(int id, UpdateUserDto dto, int currentUserId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (null, "Không tìm thấy tài khoản.");
            if (!IsValidRole(dto.Role)) return (null, "Vai trò không hợp lệ.");

            if (id == currentUserId && (dto.Role != "Admin" || !dto.IsActive))
                return (null, "Bạn không thể tự hạ quyền hoặc khoá tài khoản của chính mình.");

            if ((dto.Role != "Admin" || !dto.IsActive) && await IsLastActiveAdminAsync(user))
                return (null, "Hệ thống phải còn ít nhất một tài khoản Admin đang hoạt động.");

            user.FullName = dto.FullName.Trim();
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return (await GetByIdAsync(id), null);
        }

        public async Task<(UserDto? data, string? error)> ToggleActiveAsync(int id, int currentUserId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (null, "Không tìm thấy tài khoản.");

            if (user.IsActive)
            {
                if (id == currentUserId)
                    return (null, "Bạn không thể tự khoá tài khoản của chính mình.");

                if (await IsLastActiveAdminAsync(user))
                    return (null, "Hệ thống phải còn ít nhất một tài khoản Admin đang hoạt động.");
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(id), null);
        }

        public async Task<(bool success, string? error)> ResetPasswordAsync(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}