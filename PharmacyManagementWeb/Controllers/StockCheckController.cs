using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StockCheckController : Controller
    {
        private readonly IApiClient _apiClient;

        public StockCheckController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<List<StockCheckModel>>("/api/StockCheck");

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<StockCheckModel>());
            }

            return View(result.Data ?? new List<StockCheckModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var result = await _apiClient.GetAsync<List<MedicineModel>>("/api/Medicine");

            if (!result.Success) ViewBag.Error = result.ErrorMessage;

            var medicines = (result.Data ?? new List<MedicineModel>())
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToList();

            return View(medicines);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string itemsJson, string? note)
        {
            List<StockCheckItemInput>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<StockCheckItemInput>>(itemsJson ?? "[]", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                items = null;
            }

            if (items == null || !items.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng nhập số đếm thực tế của ít nhất 1 thuốc.";
                return RedirectToAction(nameof(Create));
            }

            var result = await _apiClient.PostAsync<StockCheckModel>("/api/StockCheck", new
            {
                items = items.Select(i => new { medicineId = i.MedicineId, actualQty = i.ActualQty }),
                note
            });

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể lưu phiếu kiểm kê.";
                return RedirectToAction(nameof(Create));
            }

            TempData["SuccessMessage"] = $"Đã lưu phiếu {result.Data.Code}. Có {result.Data.DifferenceCount} thuốc chênh lệch, tồn kho đã được điều chỉnh.";
            return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _apiClient.GetAsync<StockCheckModel>($"/api/StockCheck/{id}");
            if (!result.Success || result.Data == null) return NotFound();
            return View(result.Data);
        }
    }
}