using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;
using System.Security.Claims;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(idClaim!);
        }

        // GET /api/Order?customerId=1&fromDate=2026-01-01&toDate=2026-12-31&userId=3
        // Nhân viên chỉ xem được hoá đơn do chính mình bán (bỏ qua userId truyền lên).
        // Admin xem được tất cả và có thể lọc theo từng nhân viên bằng userId.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? customerId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? userId)
        {
            var sellerId = User.IsInRole("Admin") ? userId : GetCurrentUserId();
            return Ok(await _service.GetAllAsync(customerId, fromDate, toDate, sellerId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy hoá đơn." });

            // Nhân viên không được xem hoá đơn của người bán khác
            if (!User.IsInRole("Admin") && result.UserId != GetCurrentUserId())
                return NotFound(new { message = "Không tìm thấy hoá đơn." });

            return Ok(result);
        }

        // Chỉ Nhân viên bán hàng được tạo hoá đơn. Admin không bán hàng, chỉ xem doanh thu.
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (User.IsInRole("Admin"))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Quản trị viên không bán hàng. Vui lòng dùng tài khoản nhân viên." });

            var userId = GetCurrentUserId();
            var (data, error) = await _service.CreateAsync(dto, userId, false);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        // POST /api/Order/5/cancel - hủy hoá đơn (hoàn tiền, trả thuốc về kho)
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            // Nhân viên chỉ được hủy hoá đơn của chính mình
            if (!User.IsInRole("Admin"))
            {
                var existing = await _service.GetByIdAsync(id);
                if (existing != null && existing.UserId != GetCurrentUserId())
                    return NotFound(new { message = "Không tìm thấy hoá đơn." });
            }

            var (data, error) = await _service.CancelAsync(id);
            if (error != null)
            {
                if (error == "Không tìm thấy hoá đơn.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }
    }
}