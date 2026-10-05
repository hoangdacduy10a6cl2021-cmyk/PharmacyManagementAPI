using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class AlertController : Controller
    {
        private readonly IApiClient _apiClient;

        public AlertController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<AlertsModel>("/api/Report/alerts");

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new AlertsModel());
            }

            return View(result.Data ?? new AlertsModel());
        }
    }
}