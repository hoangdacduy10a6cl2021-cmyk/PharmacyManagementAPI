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
    public class ReturnController : ControllerBase
    {
        private readonly IReturnService _service;

        public ReturnController(IReturnService service)
        {
            _service = service;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            return Ok(await _service.GetAllAsync(status));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy phiếu trả hàng." });
            return Ok(result);
        }

        // GET /api/Return/lookup/HD0006 - tra hoá đơn và số lượng còn được phép trả
        [HttpGet("lookup/{code}")]
        public async Task<IActionResult> Lookup(string code)
        {
            var (data, error) = await _service.LookupAsync(code);
            if (error != null) return BadRequest(new { message = error });
            return Ok(data);
        }

        // Nhân viên tạo -> chờ duyệt; Admin tạo -> duyệt luôn
        [HttpPost]
        public async Task<IActionResult> Create(CreateReturnDto dto)
        {
            var (data, error) = await _service.CreateAsync(dto, GetCurrentUserId(), User.IsInRole("Admin"));
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var (data, error) = await _service.ApproveAsync(id, GetCurrentUserId());
            if (error != null) return BadRequest(new { message = error });
            return Ok(data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, ReviewReturnDto dto)
        {
            var (data, error) = await _service.RejectAsync(id, GetCurrentUserId(), dto.Note);
            if (error != null) return BadRequest(new { message = error });
            return Ok(data);
        }
    }
}