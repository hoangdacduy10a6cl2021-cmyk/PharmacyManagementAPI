using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _service;

        public PrescriptionController(IPrescriptionService service)
        {
            _service = service;
        }

        // GET /api/Prescription?search=Nguyễn&status=Đang sử dụng
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
        {
            return Ok(await _service.GetAllAsync(search, status));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy đơn thuốc." });
            return Ok(result);
        }

        // Cả Admin và Nhân viên/Dược sĩ đều được nhập đơn thuốc
        [HttpPost]
        public async Task<IActionResult> Create(CreatePrescriptionDto dto)
        {
            var (data, error) = await _service.CreateAsync(dto);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        // PATCH /api/Prescription/1/status - đổi trạng thái "Đang sử dụng" <-> "Đã giao"
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdatePrescriptionStatusDto dto)
        {
            var (data, error) = await _service.UpdateStatusAsync(id, dto);
            if (error != null)
            {
                if (data == null && error.StartsWith("Không tìm thấy")) return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }
    }
}