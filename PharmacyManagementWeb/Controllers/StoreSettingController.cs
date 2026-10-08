using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StoreSettingController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly IStoreSettingsProvider _store;

        public StoreSettingController(IApiClient apiClient, IStoreSettingsProvider store)
        {
            _apiClient = apiClient;
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<StoreSettingModel>("/api/StoreSetting");

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không tải được cài đặt cửa hàng.";
                return View(new StoreSettingModel());
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(StoreSettingModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PutAsync<StoreSettingModel>("/api/StoreSetting", new
            {
                storeName = vm.StoreName,
                address = vm.Address,
                phone = vm.Phone,
                email = vm.Email,
                taxCode = vm.TaxCode,
                lowStockThreshold = vm.LowStockThreshold,
                expiringSoonDays = vm.ExpiringSoonDays,
                maxStaffDiscountPercent = vm.MaxStaffDiscountPercent,
                receiptFooter = vm.ReceiptFooter
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể lưu cài đặt.");
                return View(vm);
            }

            _store.Invalidate();
            TempData["SuccessMessage"] = "Đã lưu cài đặt cửa hàng.";
            return RedirectToAction(nameof(Index));
        }
    }
}