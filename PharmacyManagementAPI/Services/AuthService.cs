using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<bool> AnyUserExistsAsync();
        Task<ProfileDto?> GetProfileAsync(int userId);
        Task<(ProfileDto? data, string? error)> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<(bool success, string? error)> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<bool> AnyUserExistsAsync()
        {
            return await _context.Users.AnyAsync();
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (exists) return null;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !user.IsActive) return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        private static ProfileDto ToProfile(User u) => new ProfileDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        };

        public async Task<ProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : ToProfile(user);
        }

        public async Task<(ProfileDto? data, string? error)> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (null, "Không tìm thấy tài khoản.");

            user.FullName = dto.FullName.Trim();
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

            await _context.SaveChangesAsync();
            return (ToProfile(user), null);
        }

        public async Task<(bool success, string? error)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return (false, "Mật khẩu hiện tại không đúng.");

            if (dto.CurrentPassword == dto.NewPassword)
                return (false, "Mật khẩu mới phải khác mật khẩu hiện tại.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}