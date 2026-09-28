using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly IApiClient _apiClient;

        public CustomerController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = $"/api/Customer?search={Uri.EscapeDataString(search ?? "")}";
            var result = await _apiClient.GetAsync<List<CustomerModel>>(query);

            ViewBag.Search = search;

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<CustomerModel>());
            }

            return View(result.Data ?? new List<CustomerModel>());
        }

        public IActionResult Create()
        {
            return View(new CustomerFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PostAsync<CustomerModel>("/api/Customer", new
            {
                name = vm.Name,
                phone = vm.Phone,
                address = vm.Address
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo khách hàng.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã thêm khách hàng '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _apiClient.GetAsync<CustomerModel>($"/api/Customer/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            var c = result.Data;
            var vm = new CustomerFormViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                CustomerType = c.CustomerType
            };

            ViewBag.TotalSpent = c.TotalSpent;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PutAsync<CustomerModel>($"/api/Customer/{id}", new
            {
                name = vm.Name,
                phone = vm.Phone,
                address = vm.Address,
                customerType = vm.CustomerType
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể cập nhật khách hàng.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật khách hàng '{vm.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _apiClient.DeleteAsync($"/api/Customer/{id}");
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Đã xoá khách hàng thành công." : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }
    }
}