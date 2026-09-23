using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IPrescriptionService
    {
        Task<List<PrescriptionDto>> GetAllAsync(string? search, string? status);
        Task<PrescriptionDto?> GetByIdAsync(int id);
        Task<(PrescriptionDto? data, string? error)> CreateAsync(CreatePrescriptionDto dto);
        Task<(PrescriptionDto? data, string? error)> UpdateStatusAsync(int id, UpdatePrescriptionStatusDto dto);
    }

    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static PrescriptionDto ToDto(Prescription p) => new PrescriptionDto
        {
            Id = p.Id,
            Code = p.Code,
            PatientName = p.PatientName,
            Age = p.Age,
            Gender = p.Gender,
            DoctorName = p.DoctorName,
            Diagnosis = p.Diagnosis,
            PrescriptionDate = p.PrescriptionDate,
            Status = p.Status,
            Note = p.Note,
            Details = p.PrescriptionDetails.Select(d => new PrescriptionDetailDto
            {
                Id = d.Id,
                MedicineId = d.MedicineId,
                MedicineName = d.Medicine?.Name,
                MedicineCode = d.Medicine?.Code,
                Dosage = d.Dosage,
                Quantity = d.Quantity
            }).ToList()
        };

        public async Task<List<PrescriptionDto>> GetAllAsync(string? search, string? status)
        {
            var query = _context.Prescriptions
                .Include(p => p.PrescriptionDetails).ThenInclude(d => d.Medicine)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p => p.PatientName.ToLower().Contains(search) || p.Code.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            var list = await query.OrderByDescending(p => p.PrescriptionDate).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<PrescriptionDto?> GetByIdAsync(int id)
        {
            var p = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails).ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);

            return p == null ? null : ToDto(p);
        }

        public async Task<(PrescriptionDto? data, string? error)> CreateAsync(CreatePrescriptionDto dto)
        {
            var medicineIds = dto.Details.Select(d => d.MedicineId).Distinct().ToList();
            var validCount = await _context.Medicines.CountAsync(m => medicineIds.Contains(m.Id));
            if (validCount != medicineIds.Count)
                return (null, "Có thuốc trong đơn không tồn tại.");

            var lastId = await _context.Prescriptions.CountAsync();
            var code = $"RX{(lastId + 1):D4}"; // RX0001, RX0002...

            var prescription = new Prescription
            {
                Code = code,
                PatientName = dto.PatientName,
                Age = dto.Age,
                Gender = dto.Gender,
                DoctorName = dto.DoctorName,
                Diagnosis = dto.Diagnosis,
                Note = dto.Note,
                PrescriptionDate = DateTime.Now,
                Status = "Đang sử dụng"
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            foreach (var item in dto.Details)
            {
                _context.PrescriptionDetails.Add(new PrescriptionDetail
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = item.MedicineId,
                    Dosage = item.Dosage,
                    Quantity = item.Quantity
                });
            }

            await _context.SaveChangesAsync();

            var result = await GetByIdAsync(prescription.Id);
            return (result, null);
        }

        public async Task<(PrescriptionDto? data, string? error)> UpdateStatusAsync(int id, UpdatePrescriptionStatusDto dto)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return (null, "Không tìm thấy đơn thuốc.");

            if (dto.Status != "Đang sử dụng" && dto.Status != "Đã giao")
                return (null, "Trạng thái không hợp lệ. Chỉ chấp nhận 'Đang sử dụng' hoặc 'Đã giao'.");

            prescription.Status = dto.Status;
            await _context.SaveChangesAsync();

            var result = await GetByIdAsync(id);
            return (result, null);
        }
    }
}