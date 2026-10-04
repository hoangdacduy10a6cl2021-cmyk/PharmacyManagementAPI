using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // Cho phép gọi không cần token, nhưng bên trong tự kiểm tra:
        // - Hệ thống CHƯA có tài khoản nào  -> cho tạo tài khoản đầu tiên (bắt buộc role Admin)
        // - Hệ thống ĐÃ có tài khoản        -> chỉ Admin đang đăng nhập mới được tạo tài khoản mới
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Role != "Admin" && dto.Role != "NhanVien")
                return BadRequest(new { message = "Role không hợp lệ. Chỉ chấp nhận 'Admin' hoặc 'NhanVien'." });

            var hasUsers = await _authService.AnyUserExistsAsync();

            if (hasUsers)
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("Từ chối đăng ký tài khoản '{Username}': chưa đăng nhập", dto.Username);
                    return Unauthorized(new { message = "Cần đăng nhập bằng tài khoản Admin để tạo tài khoản mới." });
                }

                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Từ chối đăng ký tài khoản '{Username}': người gọi '{Caller}' không phải Admin",
                        dto.Username, User.Identity?.Name);
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Chỉ Admin mới được tạo tài khoản mới." });
                }
            }
            else
            {
                // Tài khoản đầu tiên của hệ thống luôn là Admin
                dto.Role = "Admin";
                _logger.LogInformation("Tạo tài khoản Admin đầu tiên của hệ thống: '{Username}'", dto.Username);
            }

            var result = await _authService.RegisterAsync(dto);
            if (result == null) return BadRequest(new { message = "Username đã tồn tại." });

            _logger.LogInformation("Đã tạo tài khoản '{Username}' với role {Role}", result.Username, result.Role);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                _logger.LogWarning("Đăng nhập thất bại với username '{Username}'", dto.Username);
                return Unauthorized(new { message = "Sai username hoặc password." });
            }
            return Ok(result);
        }
    }
}