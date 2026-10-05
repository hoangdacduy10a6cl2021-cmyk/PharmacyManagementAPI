using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementAPI.Services;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IDashboardService _service;

        public ReportController(IDashboardService service)
        {
            _service = service;
        }

        // Trang chủ: cả Admin và Nhân viên
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            return Ok(await _service.GetDashboardAsync());
        }

        // Cảnh báo tồn kho / hạn dùng + chuông thông báo: cả Admin và Nhân viên
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            return Ok(await _service.GetAlertsAsync());
        }

        // Các báo cáo dưới đây chỉ dành cho Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (toDate < fromDate)
                return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });

            return Ok(await _service.GetRevenueReportAsync(fromDate, toDate));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("profit")]
        public async Task<IActionResult> GetProfitReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (toDate < fromDate)
                return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });

            return Ok(await _service.GetProfitReportAsync(fromDate, toDate));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("payments")]
        public async Task<IActionResult> GetPaymentReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (toDate < fromDate)
                return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });

            return Ok(await _service.GetPaymentReportAsync(fromDate, toDate));
        }
    }
}