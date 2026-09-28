using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly IApiClient _apiClient;

        public CategoryController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<List<CategoryModel>>("/api/Category");
            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<CategoryModel>());
            }
            return View(result.Data ?? new List<CategoryModel>());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new CategoryFormViewModel());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PostAsync<CategoryModel>("/api/Category", new { name = vm.Name, description = vm.Description });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo danh mục.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã thêm danh mục '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _apiClient.GetAsync<CategoryModel>($"/api/Category/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            var vm = new CategoryFormViewModel { Id = result.Data.Id, Name = result.Data.Name, Description = result.Data.Description };
            ViewBag.MedicineCount = result.Data.MedicineCount;
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PutAsync<CategoryModel>($"/api/Category/{id}", new { name = vm.Name, description = vm.Description });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể cập nhật danh mục.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật danh mục '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _apiClient.DeleteAsync($"/api/Category/{id}");
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Đã xoá danh mục thành công." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
    }
}