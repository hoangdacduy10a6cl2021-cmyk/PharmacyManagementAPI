using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoreSettingController : ControllerBase
    {
        private readonly IStoreSettingService _service;

        public StoreSettingController(IStoreSettingService service)
        {
            _service = service;
        }

        // Cả Admin và nhân viên đều cần đọc (tên cửa hàng, ngưỡng cảnh báo...)
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateStoreSettingDto dto)
        {
            var (data, error) = await _service.UpdateAsync(dto);
            if (error != null) return BadRequest(new { message = error });
            return Ok(data);
        }
    }
}