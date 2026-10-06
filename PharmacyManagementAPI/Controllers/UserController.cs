using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;
using System.Security.Claims;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? role)
        {
            return Ok(await _service.GetAllAsync(search, role));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản." });
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var (data, error) = await _service.CreateAsync(dto);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto dto)
        {
            var (data, error) = await _service.UpdateAsync(id, dto, GetCurrentUserId());
            if (error != null)
            {
                if (error == "Không tìm thấy tài khoản.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }

        [HttpPost("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (data, error) = await _service.ToggleActiveAsync(id, GetCurrentUserId());
            if (error != null)
            {
                if (error == "Không tìm thấy tài khoản.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordDto dto)
        {
            var (success, error) = await _service.ResetPasswordAsync(id, dto.NewPassword);
            if (!success) return NotFound(new { message = error });
            return Ok(new { message = "Đã đặt lại mật khẩu." });
        }
    }
}