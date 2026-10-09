using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Services;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicineBatchController : ControllerBase
    {
        private readonly IBatchService _service;

        public MedicineBatchController(IBatchService service)
        {
            _service = service;
        }

        // GET /api/MedicineBatch/medicine/5 - các lô của một thuốc
        [HttpGet("medicine/{medicineId:int}")]
        public async Task<IActionResult> GetByMedicine(int medicineId)
        {
            return Ok(await _service.GetBatchesAsync(medicineId));
        }

        // POST /api/MedicineBatch/5/dispose - hủy lô hết hạn (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/dispose")]
        public async Task<IActionResult> Dispose(int id)
        {
            var (data, error) = await _service.DisposeAsync(id);
            if (error != null)
            {
                if (error == "Không tìm thấy lô thuốc.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }
    }
}