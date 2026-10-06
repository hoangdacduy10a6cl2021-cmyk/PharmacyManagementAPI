using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class ReturnController : Controller
    {
        private readonly IApiClient _apiClient;

        public ReturnController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? status, string? search)
        {
            var result = await _apiClient.GetAsync<List<ReturnOrderModel>>("/api/Return");
            var all = result.Data ?? new List<ReturnOrderModel>();

            if (!result.Success) ViewBag.Error = result.ErrorMessage;

            // Số liệu tổng quan (trước khi lọc)
            ViewBag.PendingCount = all.Count(r => r.Status == "Chờ duyệt");
            ViewBag.ApprovedCount = all.Count(r => r.Status == "Đã duyệt");
            ViewBag.RejectedCount = all.Count(r => r.Status == "Từ chối");
            ViewBag.RefundTotal = all.Where(r => r.Status == "Đã duyệt").Sum(r => r.RefundAmount);

            IEnumerable<ReturnOrderModel> filtered = all;

            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(r => r.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtered = filtered.Where(r => r.Code.ToLower().Contains(s)
                    || r.OrderCode.ToLower().Contains(s)
                    || (r.CustomerName ?? "").ToLower().Contains(s));
            }

            ViewBag.Status = status;
            ViewBag.Search = search;
            return View(filtered.ToList());
        }

        // GET /Return/Create?code=HD0006
        [HttpGet]
        public async Task<IActionResult> Create(string? code)
        {
            ViewBag.Code = code;

            if (string.IsNullOrWhiteSpace(code))
                return View(default(ReturnableOrderModel));

            var result = await _apiClient.GetAsync<ReturnableOrderModel>($"/api/Return/lookup/{Uri.EscapeDataString(code.Trim())}");

            if (!result.Success || result.Data == null)
            {
                ViewBag.Error = result.ErrorMessage ?? "Không tìm thấy hoá đơn.";
                return View(default(ReturnableOrderModel));
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderId, string itemsJson, string reason, string? note, bool restockItems, string? refundMethod, string? code)
        {
            List<ReturnItemInput>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<ReturnItemInput>>(itemsJson ?? "[]", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                items = null;
            }

            if (items == null || !items.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 thuốc và nhập số lượng trả.";
                return RedirectToAction(nameof(Create), new { code });
            }

            var result = await _apiClient.PostAsync<ReturnOrderModel>("/api/Return", new
            {
                orderId,
                items = items.Select(i => new { orderDetailId = i.OrderDetailId, quantity = i.Quantity }),
                reason,
                note,
                restockItems,
                refundMethod
            });

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tạo phiếu trả hàng.";
                return RedirectToAction(nameof(Create), new { code });
            }

            TempData["SuccessMessage"] = result.Data.Status == "Đã duyệt"
                ? $"Đã tạo và duyệt phiếu {result.Data.Code}. Hoàn tiền {result.Data.RefundAmount:N0}đ."
                : $"Đã tạo phiếu {result.Data.Code}. Phiếu đang chờ Admin duyệt.";

            return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _apiClient.GetAsync<ReturnOrderModel>($"/api/Return/{id}");
            if (!result.Success || result.Data == null) return NotFound();
            return View(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _apiClient.PostAsync<ReturnOrderModel>($"/api/Return/{id}/approve", new { });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể duyệt phiếu.";
            else
                TempData["SuccessMessage"] = $"Đã duyệt phiếu {result.Data?.Code}. Hoàn tiền {result.Data?.RefundAmount:N0}đ.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? note)
        {
            var result = await _apiClient.PostAsync<ReturnOrderModel>($"/api/Return/{id}/reject", new { note });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể từ chối phiếu.";
            else
                TempData["SuccessMessage"] = $"Đã từ chối phiếu {result.Data?.Code}.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}