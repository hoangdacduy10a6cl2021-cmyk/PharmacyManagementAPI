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

        // GET /api/Order?customerId=1&fromDate=2026-01-01&toDate=2026-12-31
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? customerId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            return Ok(await _service.GetAllAsync(customerId, fromDate, toDate));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy hoá đơn." });
            return Ok(result);
        }

        // Cả Admin và Nhân viên đều được bán hàng
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var userId = GetCurrentUserId();
            var (data, error) = await _service.CreateAsync(dto, userId, User.IsInRole("Admin"));
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        // POST /api/Order/5/cancel - hủy hoá đơn (hoàn tiền, trả thuốc về kho)
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
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