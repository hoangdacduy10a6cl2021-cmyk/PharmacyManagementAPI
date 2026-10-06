using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IApiClient _apiClient;

        public AuditLogController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? search, string? status)
        {
            var from = (fromDate ?? DateTime.Today.AddDays(-6)).Date;
            var to = (toDate ?? DateTime.Today).Date;
            var toEnd = to.AddDays(1).AddSeconds(-1);

            var fromText = Uri.EscapeDataString(from.ToString("yyyy-MM-dd'T'HH:mm:ss"));
            var toText = Uri.EscapeDataString(toEnd.ToString("yyyy-MM-dd'T'HH:mm:ss"));

            var query = $"/api/AuditLog?fromDate={fromText}&toDate={toText}&search={Uri.EscapeDataString(search ?? "")}";
            if (status == "success") query += "&success=true";
            else if (status == "failed") query += "&success=false";

            var result = await _apiClient.GetAsync<List<AuditLogModel>>(query);

            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.Search = search;
            ViewBag.Status = status;

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<AuditLogModel>());
            }

            return View(result.Data ?? new List<AuditLogModel>());
        }
    }
}