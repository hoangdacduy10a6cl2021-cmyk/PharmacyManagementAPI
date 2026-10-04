using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class MedicineController : Controller
    {
        private readonly IApiClient _apiClient;

        public MedicineController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, bool? lowStock, bool? expiringSoon)
        {
            var query = $"/api/Medicine?search={Uri.EscapeDataString(search ?? "")}";
            if (categoryId.HasValue) query += $"&categoryId={categoryId}";
            if (lowStock == true) query += "&lowStock=true";
            if (expiringSoon == true) query += "&expiringSoon=true";

            var result = await _apiClient.GetAsync<List<MedicineModel>>(query);
            var categoriesResult = await _apiClient.GetAsync<List<CategoryModel>>("/api/Category");

            ViewBag.Categories = categoriesResult.Data ?? new List<CategoryModel>();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.LowStock = lowStock;
            ViewBag.ExpiringSoon = expiringSoon;

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<MedicineModel>());
            }

            return View(result.Data ?? new List<MedicineModel>());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new MedicineFormViewModel();
            await LoadDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm);
                return View(vm);
            }

            var result = await _apiClient.PostAsync<MedicineModel>("/api/Medicine", new
            {
                code = vm.Code,
                name = vm.Name,
                categoryId = vm.CategoryId,
                supplierId = vm.SupplierId,
                unit = vm.Unit,
                sellPrice = vm.SellPrice,
                importPrice = vm.ImportPrice,
                stock = vm.Stock,
                expiryDate = vm.ExpiryDate,
                barcode = vm.Barcode
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo thuốc.");
                await LoadDropdowns(vm);
                return View(vm);
            }
            if (vm.ImageFile != null && vm.ImageFile.Length > 0 && result.Data != null)
            {
                var imgResult = await _apiClient.PostFileAsync<MedicineModel>($"/api/Medicine/{result.Data.Id}/image", vm.ImageFile);
                if (!imgResult.Success)
                    TempData["ErrorMessage"] = $"Đã thêm thuốc nhưng chưa lưu được ảnh: {imgResult.ErrorMessage}";
            }
            TempData["SuccessMessage"] = $"Đã thêm thuốc '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _apiClient.GetAsync<MedicineModel>($"/api/Medicine/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            var m = result.Data;
            var vm = new MedicineFormViewModel
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                CategoryId = m.CategoryId,
                SupplierId = m.SupplierId,
                Unit = m.Unit,
                SellPrice = m.SellPrice,
                ImportPrice = m.ImportPrice,
                Stock = m.Stock,
                ExpiryDate = m.ExpiryDate,
                Barcode = m.Barcode,
                ImageUrl = m.ImageUrl,
                IsActive = m.IsActive
            };

            await LoadDropdowns(vm);
            return View(vm);
            await RestoreImageUrl(vm, id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicineFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm);
                return View(vm);
            }

            // Lưu ý: Stock không gửi lên đây vì tồn kho chỉ thay đổi qua Phiếu nhập / Bán hàng
            var result = await _apiClient.PutAsync<MedicineModel>($"/api/Medicine/{id}", new
            {
                name = vm.Name,
                categoryId = vm.CategoryId,
                supplierId = vm.SupplierId,
                unit = vm.Unit,
                sellPrice = vm.SellPrice,
                importPrice = vm.ImportPrice,
                expiryDate = vm.ExpiryDate,
                barcode = vm.Barcode,
                isActive = vm.IsActive
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể cập nhật thuốc.");
                await LoadDropdowns(vm);
                return View(vm);
            }
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var imgResult = await _apiClient.PostFileAsync<MedicineModel>($"/api/Medicine/{id}/image", vm.ImageFile);
                if (!imgResult.Success)
                    TempData["ErrorMessage"] = $"Đã cập nhật thuốc nhưng chưa lưu được ảnh: {imgResult.ErrorMessage}";
            }
            else if (vm.RemoveImage)
            {
                await _apiClient.DeleteAsync($"/api/Medicine/{id}/image");
            }
            TempData["SuccessMessage"] = $"Đã cập nhật thuốc '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _apiClient.DeleteAsync($"/api/Medicine/{id}");
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Đã xoá/ngưng kinh doanh thuốc thành công." : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }
        private async Task RestoreImageUrl(MedicineFormViewModel vm, int id)
        {
            var current = await _apiClient.GetAsync<MedicineModel>($"/api/Medicine/{id}");
            vm.ImageUrl = current.Data?.ImageUrl;
        }
        private async Task LoadDropdowns(MedicineFormViewModel vm)
        {
            var categoriesResult = await _apiClient.GetAsync<List<CategoryModel>>("/api/Category");
            var suppliersResult = await _apiClient.GetAsync<List<SupplierModel>>("/api/Supplier");

            vm.Categories = categoriesResult.Data ?? new List<CategoryModel>();
            vm.Suppliers = suppliersResult.Data ?? new List<SupplierModel>();
        }
    }
}