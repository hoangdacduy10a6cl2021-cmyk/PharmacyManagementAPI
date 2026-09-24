using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IApiClient _apiClient;

        public HomeController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<DashboardModel>("/api/Report/dashboard");

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage ?? "Không thể tải dữ liệu Dashboard.";
                return View(new DashboardModel());
            }

            return View(result.Data ?? new DashboardModel());
        }
    }
}