using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicineController : ControllerBase
    {
        private readonly IMedicineService _service;

        public MedicineController(IMedicineService service)
        {
            _service = service;
        }

        // GET /api/Medicine?search=paracetamol&categoryId=1&lowStock=true&expiringSoon=true
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] bool? lowStock,
            [FromQuery] bool? expiringSoon)
        {
            return Ok(await _service.GetAllAsync(search, categoryId, lowStock, expiringSoon));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy thuốc." });
            return Ok(result);
        }

        // GET /api/Medicine/barcode/8934567890123 - dùng cho chức năng quét mã vạch
        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var result = await _service.GetByBarcodeAsync(barcode);
            if (result == null) return NotFound(new { message = "Không tìm thấy thuốc với mã vạch này." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateMedicineDto dto)
        {
            var (data, error) = await _service.CreateAsync(dto);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateMedicineDto dto)
        {
            var (data, error) = await _service.UpdateAsync(id, dto);
            if (error != null)
            {
                if (data == null && error == "Không tìm thấy thuốc.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }
        // POST /api/Medicine/5/image  (multipart/form-data, field name: file)
        [HttpPost("{id}/image")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            var (data, error) = await _service.UploadImageAsync(id, file);
            if (error != null)
            {
                if (error == "Không tìm thấy thuốc.") return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }
            return Ok(data);
        }

        // DELETE /api/Medicine/5/image
        [HttpDelete("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var (data, error) = await _service.DeleteImageAsync(id);
            if (error != null) return NotFound(new { message = error });
            return Ok(data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _service.DeleteAsync(id);
            if (!success) return BadRequest(new { message = error });

            if (error != null) return Ok(new { message = error });

            return NoContent();
        }
    }
}