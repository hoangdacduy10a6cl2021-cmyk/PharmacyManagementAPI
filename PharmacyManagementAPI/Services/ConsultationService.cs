using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IConsultationService
    {
        Task<List<ConsultationDto>> GetAllAsync();
        Task<ConsultationDto?> GetByIdAsync(int id);
        Task<(ConsultationDto? data, string? error)> CreateAsync(CreateConsultationDto dto, int userId);
        Task<(ConsultationDto? data, string? error)> CompleteAsync(int id);
    }

    public class ConsultationService : IConsultationService
    {
        private readonly ApplicationDbContext _context;

        private const string FOLLOW_UP = "Cần theo dõi";
        private const string DONE = "Hoàn tất";

        public ConsultationService(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Consultation> Query() => _context.Consultations
            .Include(c => c.CreatedBy)
            .Include(c => c.Medicines).ThenInclude(m => m.Medicine);

        private static ConsultationDto ToDto(Consultation c) => new ConsultationDto
        {
            Id = c.Id,
            Code = c.Code,
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName,
            CustomerPhone = c.CustomerPhone,
            Symptoms = c.Symptoms,
            Advice = c.Advice,
            FollowUpDate = c.FollowUpDate,
            Status = c.Status,
            CreatedByName = c.CreatedBy?.FullName,
            CreatedAt = c.CreatedAt,
            Medicines = c.Medicines.Select(m => new ConsultationMedicineDto
            {
                MedicineId = m.MedicineId,
                Name = m.Medicine?.Name ?? "",
                Code = m.Medicine?.Code ?? "",
                Unit = m.Medicine?.Unit,
                ImageUrl = m.Medicine?.ImageUrl,
                SellPrice = m.Medicine?.SellPrice ?? 0,
                Stock = m.Medicine?.Stock ?? 0
            }).ToList()
        };

        // Trả về tối đa 500 phiếu mới nhất
        public async Task<List<ConsultationDto>> GetAllAsync()
        {
            var list = await Query().OrderByDescending(c => c.CreatedAt).Take(500).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<ConsultationDto?> GetByIdAsync(int id)
        {
            var c = await Query().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : ToDto(c);
        }

        public async Task<(ConsultationDto? data, string? error)> CreateAsync(CreateConsultationDto dto, int userId)
        {
            string name;
            string? phone = string.IsNullOrWhiteSpace(dto.CustomerPhone) ? null : dto.CustomerPhone.Trim();
            int? customerId = null;

            if (dto.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(dto.CustomerId.Value);
                if (customer == null) return (null, "Khách hàng không tồn tại.");

                customerId = customer.Id;
                name = customer.Name;
                phone ??= customer.Phone;
            }
            else
            {
                name = string.IsNullOrWhiteSpace(dto.CustomerName) ? "Khách vãng lai" : dto.CustomerName.Trim();
            }

            var ids = dto.MedicineIds.Distinct().ToList();
            if (ids.Any())
            {
                var found = await _context.Medicines.CountAsync(m => ids.Contains(m.Id));
                if (found != ids.Count) return (null, "Có thuốc tư vấn không tồn tại.");
            }

            var count = await _context.Consultations.CountAsync();

            var consultation = new Consultation
            {
                Code = $"TV{(count + 1):D4}",
                CustomerId = customerId,
                CustomerName = name,
                CustomerPhone = phone,
                Symptoms = dto.Symptoms.Trim(),
                Advice = dto.Advice.Trim(),
                FollowUpDate = dto.FollowUpDate?.Date,
                Status = dto.FollowUpDate.HasValue ? FOLLOW_UP : DONE,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now,
                Medicines = ids.Select(id => new ConsultationMedicine { MedicineId = id }).ToList()
            };

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(consultation.Id), null);
        }

        public async Task<(ConsultationDto? data, string? error)> CompleteAsync(int id)
        {
            var c = await _context.Consultations.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return (null, "Không tìm thấy phiếu tư vấn.");
            if (c.Status == DONE) return (null, "Phiếu này đã hoàn tất.");

            c.Status = DONE;
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(id), null);
        }
    }
}