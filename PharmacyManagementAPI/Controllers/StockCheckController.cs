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
    public class StockCheckController : ControllerBase
    {
        private readonly IStockCheckService _service;

        public StockCheckController(IStockCheckService service)
        {
            _service = service;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy phiếu kiểm kê." });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateStockCheckDto dto)
        {
            var (data, error) = await _service.CreateAsync(dto, GetCurrentUserId());
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }
    }
}