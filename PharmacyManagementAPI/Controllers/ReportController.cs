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

        // GET /api/Report/dashboard - dùng cho màn hình Trang chủ/Dashboard (cả người dùng và admin)
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            return Ok(await _service.GetDashboardAsync());
        }

        // GET /api/Report/revenue?fromDate=2026-09-01&toDate=2026-09-22 - dùng cho màn hình Báo cáo
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            if (toDate < fromDate)
                return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });

            return Ok(await _service.GetRevenueReportAsync(fromDate, toDate));
        }
    }
}