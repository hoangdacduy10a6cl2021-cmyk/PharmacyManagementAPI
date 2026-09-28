using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;
using System.Text.Json;

namespace PharmacyManagementWeb.Controllers
{
    [Authorize]
    public class PrescriptionController : Controller
    {
        private readonly IApiClient _apiClient;

        public PrescriptionController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = $"/api/Prescription?search={Uri.EscapeDataString(search ?? "")}";
            if (!string.IsNullOrEmpty(status)) query += $"&status={Uri.EscapeDataString(status)}";

            var result = await _apiClient.GetAsync<List<PrescriptionModel>>(query);

            ViewBag.Search = search;
            ViewBag.Status = status;

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(new List<PrescriptionModel>());
            }

            return View(result.Data ?? new List<PrescriptionModel>());
        }

        public IActionResult Create()
        {
            return View(new PrescriptionFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionFormViewModel vm, string detailsJson)
        {
            List<PrescriptionDetailInput>? details;
            try
            {
                details = JsonSerializer.Deserialize<List<PrescriptionDetailInput>>(detailsJson ?? "[]");
            }
            catch
            {
                details = null;
            }

            if (!ModelState.IsValid)
                return View(vm);

            if (details == null || !details.Any())
            {
                ModelState.AddModelError(string.Empty, "Đơn thuốc phải có ít nhất 1 loại thuốc.");
                return View(vm);
            }

            var result = await _apiClient.PostAsync<PrescriptionModel>("/api/Prescription", new
            {
                patientName = vm.PatientName,
                age = vm.Age,
                gender = vm.Gender,
                doctorName = vm.DoctorName,
                diagnosis = vm.Diagnosis,
                note = vm.Note,
                details = details.Select(d => new { medicineId = d.MedicineId, dosage = d.Dosage, quantity = d.Quantity })
            });

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể tạo đơn thuốc.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Đã tạo đơn thuốc {result.Data.Code} thành công.";
            return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _apiClient.GetAsync<PrescriptionModel>($"/api/Prescription/{id}");
            if (!result.Success || result.Data == null) return NotFound();
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var result = await _apiClient.PatchAsync<PrescriptionModel>($"/api/Prescription/{id}/status", new { status });

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Đã cập nhật trạng thái đơn thuốc." : result.ErrorMessage;

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}