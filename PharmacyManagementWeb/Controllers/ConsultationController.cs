using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private const string FOLLOW_UP = "Cần theo dõi";
        private readonly IApiClient _apiClient;

        public ConsultationController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? status, string? search)
        {
            var result = await _apiClient.GetAsync<List<ConsultationModel>>("/api/Consultation");
            var all = result.Data ?? new List<ConsultationModel>();

            if (!result.Success) ViewBag.Error = result.ErrorMessage;

            // Số liệu tổng quan (trước khi lọc)
            var today = DateTime.Today;
            ViewBag.TotalCount = all.Count;
            ViewBag.TodayCount = all.Count(c => c.CreatedAt.Date == today);
            ViewBag.FollowUpCount = all.Count(c => c.Status == FOLLOW_UP);
            ViewBag.OverdueCount = all.Count(c => c.Status == FOLLOW_UP && c.FollowUpDate.HasValue && c.FollowUpDate.Value.Date < today);

            IEnumerable<ConsultationModel> filtered = all;

            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(c => c.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtered = filtered.Where(c => c.Code.ToLower().Contains(s)
                    || c.CustomerName.ToLower().Contains(s)
                    || (c.CustomerPhone ?? "").Contains(s)
                    || c.Symptoms.ToLower().Contains(s));
            }

            ViewBag.Status = status;
            ViewBag.Search = search;
            return View(filtered.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? customerId)
        {
            var customers = await _apiClient.GetAsync<List<CustomerModel>>("/api/Customer");
            ViewBag.Customers = customers.Data ?? new List<CustomerModel>();
            ViewBag.SelectedCustomerId = customerId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? customerId, string? customerName, string? customerPhone,
            string symptoms, string advice, DateTime? followUpDate, string? medicineIdsJson)
        {
            if (string.IsNullOrWhiteSpace(symptoms) || string.IsNullOrWhiteSpace(advice))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập triệu chứng / yêu cầu của khách và nội dung tư vấn.";
                return RedirectToAction(nameof(Create), new { customerId });
            }

            List<int> medicineIds;
            try
            {
                medicineIds = JsonSerializer.Deserialize<List<int>>(medicineIdsJson ?? "[]") ?? new List<int>();
            }
            catch
            {
                medicineIds = new List<int>();
            }

            var result = await _apiClient.PostAsync<ConsultationModel>("/api/Consultation", new
            {
                customerId,
                customerName,
                customerPhone,
                symptoms,
                advice,
                followUpDate,
                medicineIds
            });

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể lưu phiếu tư vấn.";
                return RedirectToAction(nameof(Create), new { customerId });
            }

            TempData["SuccessMessage"] = $"Đã lưu phiếu tư vấn {result.Data.Code}.";
            return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _apiClient.GetAsync<ConsultationModel>($"/api/Consultation/{id}");
            if (!result.Success || result.Data == null) return NotFound();
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _apiClient.PostAsync<ConsultationModel>($"/api/Consultation/{id}/complete", new { });

            if (!result.Success)
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Không thể hoàn tất phiếu.";
            else
                TempData["SuccessMessage"] = $"Đã hoàn tất phiếu {result.Data?.Code}.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}