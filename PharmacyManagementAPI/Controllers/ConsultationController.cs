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
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationService _service;

        public ConsultationController(IConsultationService service)
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
            if (result == null) return NotFound(new { message = "Không tìm thấy phiếu tư vấn." });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateConsultationDto dto)
        {
            var (data, error) = await _service.CreateAsync(dto, GetCurrentUserId());
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        [HttpPost("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var (data, error) = await _service.CompleteAsync(id);
            if (error != null)
            {
                if (error == "Không tìm thấy phiếu tư vấn.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }
    }
}