using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IApiClient _apiClient;

        public OrderController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET /Order - Màn hình bán hàng (POS)
        public async Task<IActionResult> Index()
        {
            var customersResult = await _apiClient.GetAsync<List<CustomerModel>>("/api/Customer");
            ViewBag.Customers = customersResult.Data ?? new List<CustomerModel>();
            return View();
        }

        // GET /Order/SearchMedicine?term=... - AJAX tìm thuốc để thêm vào giỏ hàng
        [HttpGet]
        public async Task<IActionResult> SearchMedicine(string? term)
        {
            var result = await _apiClient.GetAsync<List<MedicineModel>>($"/api/Medicine?search={Uri.EscapeDataString(term ?? "")}");
            var list = (result.Data ?? new List<MedicineModel>())
                .Where(m => m.IsActive)
                .Select(m => new
                {
                    id = m.Id,
                    code = m.Code,
                    name = m.Name,
                    unit = m.Unit,
                    sellPrice = m.SellPrice,
                    stock = m.Stock
                });

            return Json(list);
        }

        // POST /Order/QuickCreateCustomer - AJAX tạo nhanh khách hàng ngay tại màn hình bán hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateCustomer(string name, string? phone, string? address)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Tên khách hàng không được để trống." });

            var result = await _apiClient.PostAsync<CustomerModel>("/api/Customer", new { name, phone, address });

            if (!result.Success || result.Data == null)
                return BadRequest(new { message = result.ErrorMessage ?? "Không thể tạo khách hàng." });

            return Json(new { id = result.Data.Id, name = result.Data.Name });
        }

        // POST /Order/Create - tạo hoá đơn từ giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? customerId, string cartJson, decimal discountAmount, string paymentMethod)
        {
            List<CartItemInput>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<CartItemInput>>(cartJson ?? "[]");
            }
            catch
            {
                items = null;
            }

            if (items == null || !items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng đang trống, vui lòng chọn ít nhất 1 thuốc.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiClient.PostAsync<OrderModel>("/api/Order", new
            {
                customerId,
                details = items.Select(i => new { medicineId = i.MedicineId, quantity = i.Quantity }),
                discountAmount,
                paymentMethod
            });

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tạo hoá đơn.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Tạo hoá đơn {result.Data.Code} thành công! Tổng tiền: {result.Data.FinalAmount:N0}đ";
            return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
        }

        // GET /Order/Detail/5 - xem hoá đơn (sau khi bán / tra cứu lại)
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _apiClient.GetAsync<OrderModel>($"/api/Order/{id}");
            if (!result.Success || result.Data == null) return NotFound();
            return View(result.Data);
        }
    }
}