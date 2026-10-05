using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Security.Claims;

namespace PharmacyManagementWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiClient _apiClient;

        public AccountController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _apiClient.PostAsync<AuthResponseModel>("/api/Auth/login", new
            {
                username = model.Username,
                password = model.Password
            });

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đăng nhập thất bại.");
                return View(model);
            }

            var auth = result.Data;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
                new Claim(ClaimTypes.Name, auth.Username),
                new Claim(ClaimTypes.Role, auth.Role),
                new Claim("FullName", auth.FullName),
                new Claim("ApiToken", auth.Token) // lưu JWT để đính vào các request gọi API sau này
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = auth.ExpiresAt.ToUniversalTime()
            });

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // ===== Tài khoản cá nhân =====

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await _apiClient.GetAsync<ProfileModel>("/api/Auth/me");
            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không tải được thông tin tài khoản.";
                return RedirectToAction("Index", "Home");
            }
            return View(result.Data);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string? email, string? phone)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["ErrorMessage"] = "Họ tên không được để trống.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _apiClient.PutAsync<ProfileModel>("/api/Auth/me", new { fullName, email, phone });

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể cập nhật thông tin.";
                return RedirectToAction(nameof(Profile));
            }

            // Cập nhật lại tên hiển thị trong cookie đăng nhập (giữ nguyên token và thời hạn)
            var current = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = User.Claims.Where(c => c.Type != "FullName").ToList();
            claims.Add(new Claim("FullName", result.Data.FullName));
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), current.Properties);

            TempData["SuccessMessage"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới.";
                return RedirectToAction(nameof(Profile));
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _apiClient.PostAsync<object>("/api/Auth/change-password", new { currentPassword, newPassword });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể đổi mật khẩu.";
            else
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";

            return RedirectToAction(nameof(Profile));
        }
    }
}