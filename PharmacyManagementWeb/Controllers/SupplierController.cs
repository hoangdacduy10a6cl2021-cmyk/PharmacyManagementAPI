using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly IApiClient _apiClient;

        public SupplierController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<List<SupplierModel>>("/api/Supplier");
            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<SupplierModel>());
            }
            return View(result.Data ?? new List<SupplierModel>());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new SupplierFormViewModel());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PostAsync<SupplierModel>("/api/Supplier", new
            {
                name = vm.Name,
                phone = vm.Phone,
                email = vm.Email,
                address = vm.Address
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo nhà cung cấp.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã thêm nhà cung cấp '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _apiClient.GetAsync<SupplierModel>($"/api/Supplier/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            var s = result.Data;
            var vm = new SupplierFormViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PutAsync<SupplierModel>($"/api/Supplier/{id}", new
            {
                name = vm.Name,
                phone = vm.Phone,
                email = vm.Email,
                address = vm.Address,
                isActive = vm.IsActive
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể cập nhật nhà cung cấp.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật nhà cung cấp '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _apiClient.DeleteAsync($"/api/Supplier/{id}");
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Đã xử lý xoá/ngưng hoạt động nhà cung cấp." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
    }
}