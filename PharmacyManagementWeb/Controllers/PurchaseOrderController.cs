using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly IApiClient _apiClient;

        public PurchaseOrderController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiClient.GetAsync<List<PurchaseOrderModel>>("/api/PurchaseOrder");
            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<PurchaseOrderModel>());
            }
            return View(result.Data ?? new List<PurchaseOrderModel>());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var suppliersResult = await _apiClient.GetAsync<List<SupplierModel>>("/api/Supplier");
            ViewBag.Suppliers = suppliersResult.Data ?? new List<SupplierModel>();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int supplierId, string cartJson)
        {
            List<PurchaseOrderCartItemInput>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<PurchaseOrderCartItemInput>>(cartJson ?? "[]", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                items = null;
            }

            if (items == null || !items.Any())
            {
                TempData["ErrorMessage"] = "Phiếu nhập phải có ít nhất 1 thuốc.";
                return RedirectToAction(nameof(Create));
            }

            var result = await _apiClient.PostAsync<PurchaseOrderModel>("/api/PurchaseOrder", new
            {
                supplierId,
                details = items.Select(i => new { medicineId = i.MedicineId, quantity = i.Quantity, importPrice = i.ImportPrice, batchNumber = i.BatchNumber, expiryDate = i.ExpiryDate })
            });

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể tạo phiếu nhập.";
                return RedirectToAction(nameof(Create));
            }

            TempData["SuccessMessage"] = $"Tạo phiếu nhập {result.Data.Code} thành công! Tổng tiền: {result.Data.TotalAmount:N0}đ";
            return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _apiClient.GetAsync<PurchaseOrderModel>($"/api/PurchaseOrder/{id}");
            if (!result.Success || result.Data == null) return NotFound();
            return View(result.Data);
        }
    }
}