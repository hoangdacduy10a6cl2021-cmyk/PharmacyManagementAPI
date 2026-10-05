using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IApiClient _apiClient;

        public ReportController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Doanh thu
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-6);
            var to = toDate ?? DateTime.Today;

            ViewBag.FromDate = from;
            ViewBag.ToDate = to;

            var query = $"/api/Report/revenue?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";
            var result = await _apiClient.GetAsync<RevenueReportModel>(query);

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new RevenueReportModel { FromDate = from, ToDate = to });
            }

            return View(result.Data ?? new RevenueReportModel { FromDate = from, ToDate = to });
        }

        // Lợi nhuận
        public async Task<IActionResult> Profit(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-6);
            var to = toDate ?? DateTime.Today;

            var result = await _apiClient.GetAsync<ProfitReportModel>(
                $"/api/Report/profit?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new ProfitReportModel { FromDate = from, ToDate = to });
            }

            return View(result.Data ?? new ProfitReportModel { FromDate = from, ToDate = to });
        }

        // Thanh toán
        public async Task<IActionResult> Payments(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-6);
            var to = toDate ?? DateTime.Today;

            var result = await _apiClient.GetAsync<PaymentReportModel>(
                $"/api/Report/payments?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new PaymentReportModel { FromDate = from, ToDate = to });
            }

            return View(result.Data ?? new PaymentReportModel { FromDate = from, ToDate = to });
        }
    }
}