using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IApiClient _apiClient;

        public ReportController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

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
    }
}