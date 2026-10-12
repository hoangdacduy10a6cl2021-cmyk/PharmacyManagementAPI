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

        // Công khai (không cần đăng nhập): chỉ trả tên cửa hàng cho trang Đăng nhập
        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> GetPublic()
        {
            var s = await _service.GetAsync();
            return Ok(new { storeName = s.StoreName });
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