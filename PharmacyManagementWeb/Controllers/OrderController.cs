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
        private const string CANCELLED = "Đã hủy";
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

        // GET /Order/History - danh sách hoá đơn
        public async Task<IActionResult> History(DateTime? fromDate, DateTime? toDate, string? search, string? status)
        {
            var from = (fromDate ?? DateTime.Today.AddDays(-29)).Date;
            var to = (toDate ?? DateTime.Today).Date;
            var toEnd = to.AddDays(1).AddSeconds(-1);

            var fromText = Uri.EscapeDataString(from.ToString("yyyy-MM-dd'T'HH:mm:ss"));
            var toText = Uri.EscapeDataString(toEnd.ToString("yyyy-MM-dd'T'HH:mm:ss"));

            var result = await _apiClient.GetAsync<List<OrderModel>>($"/api/Order?fromDate={fromText}&toDate={toText}");
            var all = result.Data ?? new List<OrderModel>();

            if (!result.Success) ViewBag.Error = result.ErrorMessage;

            // Số liệu tổng quan của cả khoảng ngày (trước khi lọc theo từ khoá / trạng thái)
            ViewBag.TotalCount = all.Count;
            ViewBag.CompletedCount = all.Count(o => o.Status != CANCELLED);
            ViewBag.CancelledCount = all.Count(o => o.Status == CANCELLED);
            ViewBag.Revenue = all.Where(o => o.Status != CANCELLED).Sum(o => o.FinalAmount);

            IEnumerable<OrderModel> filtered = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtered = filtered.Where(o => o.Code.ToLower().Contains(s)
                    || (o.CustomerName ?? "").ToLower().Contains(s)
                    || (o.UserName ?? "").ToLower().Contains(s));
            }

            if (status == "cancelled") filtered = filtered.Where(o => o.Status == CANCELLED);
            else if (status == "completed") filtered = filtered.Where(o => o.Status != CANCELLED);

            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(filtered.ToList());
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
                    stock = m.Stock,
                    imageUrl = m.ImageUrl
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
                items = JsonSerializer.Deserialize<List<CartItemInput>>(cartJson ?? "[]", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        // GET /Order/Detail/5 - xem hoá đơn (?print=true để mở sẵn hộp thoại in)
        public async Task<IActionResult> Detail(int id, bool print = false)
        {
            var result = await _apiClient.GetAsync<OrderModel>($"/api/Order/{id}");
            if (!result.Success || result.Data == null) return NotFound();

            ViewBag.AutoPrint = print;
            return View(result.Data);
        }

        // POST /Order/Cancel/5 - hủy hoá đơn (hoàn tiền)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? returnTo)
        {
            var result = await _apiClient.PostAsync<OrderModel>($"/api/Order/{id}/cancel", new { });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể hủy hoá đơn.";
            else
                TempData["SuccessMessage"] = $"Đã hủy hoá đơn {result.Data?.Code}. Thuốc đã được hoàn lại kho.";

            return returnTo == "history"
                ? RedirectToAction(nameof(History))
                : RedirectToAction(nameof(Detail), new { id });
        }
    }
}