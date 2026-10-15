using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PharmacyManagementWeb.Controllers
{
    // Nhập hàng là nghiệp vụ của Admin
    [Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> Create()
        {
            var suppliersResult = await _apiClient.GetAsync<List<SupplierModel>>("/api/Supplier");
            var categoriesResult = await _apiClient.GetAsync<List<CategoryModel>>("/api/Category");
            var medicinesResult = await _apiClient.GetAsync<List<MedicineModel>>("/api/Medicine");

            ViewBag.Suppliers = suppliersResult.Data ?? new List<SupplierModel>();
            ViewBag.Categories = categoriesResult.Data ?? new List<CategoryModel>();

            // Thuốc đã hết hàng (tồn kho = 0) để nhập lại nhanh
            ViewBag.OutOfStock = (medicinesResult.Data ?? new List<MedicineModel>())
                .Where(m => m.IsActive && m.Stock <= 0)
                .OrderBy(m => m.Name)
                .Select(ToJson)
                .ToList();

            return View();
        }

        // GET /PurchaseOrder/SearchMedicine?term=... - AJAX tìm thuốc để thêm vào phiếu nhập
        [HttpGet]
        public async Task<IActionResult> SearchMedicine(string? term)
        {
            var result = await _apiClient.GetAsync<List<MedicineModel>>($"/api/Medicine?search={Uri.EscapeDataString(term ?? "")}");
            var list = (result.Data ?? new List<MedicineModel>())
                .Where(m => m.IsActive)
                .Select(ToJson);

            return Json(list);
        }

        // POST /PurchaseOrder/QuickCreateMedicine - AJAX tạo nhanh loại thuốc mới ngay trên form nhập hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateMedicine(string? code, string name, int categoryId, int? supplierId,
            string? unit, decimal sellPrice, decimal importPrice, string? barcode)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Tên thuốc không được để trống." });
            if (categoryId <= 0)
                return BadRequest(new { message = "Vui lòng chọn danh mục." });
            if (sellPrice < 0 || importPrice < 0)
                return BadRequest(new { message = "Giá không hợp lệ." });

            // Không nhập mã thì tự sinh mã tiếp theo: TH001, TH002...
            if (string.IsNullOrWhiteSpace(code))
            {
                var all = await _apiClient.GetAsync<List<MedicineModel>>("/api/Medicine");
                var maxNumber = (all.Data ?? new List<MedicineModel>())
                    .Select(m => Regex.Match(m.Code ?? "", @"^TH(\d+)$", RegexOptions.IgnoreCase))
                    .Where(x => x.Success)
                    .Select(x => int.Parse(x.Groups[1].Value))
                    .DefaultIfEmpty(0)
                    .Max();
                code = $"TH{(maxNumber + 1):D3}";
            }

            // Thuốc mới tạo với tồn kho 0, số lượng sẽ được cộng khi phiếu nhập được lưu (tạo lô mới)
            var result = await _apiClient.PostAsync<MedicineModel>("/api/Medicine", new
            {
                code = code.Trim(),
                name = name.Trim(),
                categoryId,
                supplierId,
                unit = string.IsNullOrWhiteSpace(unit) ? "Hộp" : unit.Trim(),
                sellPrice,
                importPrice,
                stock = 0,
                barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim()
            });

            if (!result.Success || result.Data == null)
                return BadRequest(new { message = result.ErrorMessage ?? "Không thể tạo thuốc mới." });

            return Json(ToJson(result.Data));
        }

        [HttpPost]
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

        private static object ToJson(MedicineModel m) => new
        {
            id = m.Id,
            code = m.Code,
            name = m.Name,
            unit = m.Unit,
            stock = m.Stock,
            importPrice = m.ImportPrice,
            sellPrice = m.SellPrice,
            imageUrl = m.ImageUrl,
            supplierId = m.SupplierId,
            supplierName = m.SupplierName
        };
    }
}