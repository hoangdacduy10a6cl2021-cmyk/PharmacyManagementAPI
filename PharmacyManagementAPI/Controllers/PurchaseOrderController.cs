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
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrderController(IPurchaseOrderService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(idClaim!);
        }

        // GET /api/PurchaseOrder?supplierId=1&fromDate=2026-01-01&toDate=2026-12-31
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? supplierId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            return Ok(await _service.GetAllAsync(supplierId, fromDate, toDate));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy phiếu nhập." });
            return Ok(result);
        }

        // Chỉ Admin được tạo phiếu nhập (theo mockup - phần Quản lý nhập hàng nằm bên Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreatePurchaseOrderDto dto)
        {
            var userId = GetCurrentUserId();
            var (data, error) = await _service.CreateAsync(dto, userId);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }
    }
}