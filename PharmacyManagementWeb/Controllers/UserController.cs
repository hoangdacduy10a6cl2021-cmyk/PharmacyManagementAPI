using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Security.Claims;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IApiClient _apiClient;

        public UserController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        private int CurrentUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        public async Task<IActionResult> Index(string? search, string? role)
        {
            var query = $"/api/User?search={Uri.EscapeDataString(search ?? "")}&role={Uri.EscapeDataString(role ?? "")}";
            var result = await _apiClient.GetAsync<List<UserModel>>(query);

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.CurrentUserId = CurrentUserId();

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<UserModel>());
            }

            return View(result.Data ?? new List<UserModel>());
        }

        [HttpGet]
        public IActionResult Create() => View(new UserCreateViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PostAsync<UserModel>("/api/User", new
            {
                username = vm.Username,
                password = vm.Password,
                fullName = vm.FullName,
                email = vm.Email,
                phone = vm.Phone,
                role = vm.Role
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo tài khoản.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã tạo tài khoản '{vm.Username}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _apiClient.GetAsync<UserModel>($"/api/User/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            var u = result.Data;
            ViewBag.IsSelf = u.Id == CurrentUserId();

            return View(new UserEditViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel vm)
        {
            ViewBag.IsSelf = id == CurrentUserId();
            if (!ModelState.IsValid) return View(vm);

            var result = await _apiClient.PutAsync<UserModel>($"/api/User/{id}", new
            {
                fullName = vm.FullName,
                email = vm.Email,
                phone = vm.Phone,
                role = vm.Role,
                isActive = vm.IsActive
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể cập nhật tài khoản.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật tài khoản '{vm.Username}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _apiClient.PostAsync<UserModel>($"/api/User/{id}/toggle-active", new { });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể thay đổi trạng thái tài khoản.";
            else
                TempData["SuccessMessage"] = result.Data!.IsActive
                    ? $"Đã mở khoá tài khoản '{result.Data.Username}'."
                    : $"Đã khoá tài khoản '{result.Data.Username}'.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiClient.PostAsync<object>($"/api/User/{id}/reset-password", new { newPassword });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể đặt lại mật khẩu.";
            else
                TempData["SuccessMessage"] = "Đã đặt lại mật khẩu. Hãy báo mật khẩu mới cho nhân viên.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Permissions() => View();
    }
}