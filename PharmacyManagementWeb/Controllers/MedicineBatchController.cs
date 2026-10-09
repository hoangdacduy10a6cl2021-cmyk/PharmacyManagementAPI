using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class MedicineBatchController : Controller
    {
        private readonly IApiClient _apiClient;

        public MedicineBatchController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET /MedicineBatch/Index/5 - các lô của thuốc có id = 5
        public async Task<IActionResult> Index(int id)
        {
            var medicine = await _apiClient.GetAsync<MedicineModel>($"/api/Medicine/{id}");
            if (!medicine.Success || medicine.Data == null) return NotFound();

            var batches = await _apiClient.GetAsync<List<MedicineBatchModel>>($"/api/MedicineBatch/medicine/{id}");
            if (!batches.Success) ViewBag.Error = batches.ErrorMessage;

            ViewBag.Medicine = medicine.Data;
            return View(batches.Data ?? new List<MedicineBatchModel>());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispose(int id, int medicineId)
        {
            var result = await _apiClient.PostAsync<MedicineBatchModel>($"/api/MedicineBatch/{id}/dispose", new { });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể hủy lô.";
            else
                TempData["SuccessMessage"] = $"Đã hủy lô {result.Data?.BatchNumber} do hết hạn.";

            return RedirectToAction(nameof(Index), new { id = medicineId });
        }
    }
}