using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class PromotionController : Controller
    {
        private readonly IApiClient _apiClient;

        public PromotionController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<List<PromotionModel>>("/api/Promotion");
            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<PromotionModel>());
            }
            return View(result.Data ?? new List<PromotionModel>());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new PromotionFormViewModel());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PostAsync<PromotionModel>("/api/Promotion", new
            {
                code = vm.Code,
                name = vm.Name,
                discountType = vm.DiscountType,
                discountValue = vm.DiscountValue,
                startDate = vm.StartDate,
                endDate = vm.EndDate
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo khuyến mãi.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã tạo khuyến mãi '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _apiClient.GetAsync<PromotionModel>($"/api/Promotion/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            var p = result.Data;
            var vm = new PromotionFormViewModel
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromotionFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PutAsync<PromotionModel>($"/api/Promotion/{id}", new
            {
                name = vm.Name,
                discountType = vm.DiscountType,
                discountValue = vm.DiscountValue,
                startDate = vm.StartDate,
                endDate = vm.EndDate,
                isActive = vm.IsActive
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể cập nhật khuyến mãi.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật khuyến mãi '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _apiClient.DeleteAsync($"/api/Promotion/{id}");
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Đã xoá khuyến mãi thành công." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
    }
}