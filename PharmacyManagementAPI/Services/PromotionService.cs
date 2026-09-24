using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IPromotionService
    {
        Task<List<PromotionDto>> GetAllAsync();
        Task<PromotionDto?> GetByIdAsync(int id);
        Task<(PromotionDto? data, string? error)> CreateAsync(CreatePromotionDto dto);
        Task<(PromotionDto? data, string? error)> UpdateAsync(int id, UpdatePromotionDto dto);
        Task<(bool success, string? error)> DeleteAsync(int id);
    }

    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;

        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string ComputeCurrentStatus(Promotion p)
        {
            var now = DateTime.Now;
            if (!p.IsActive) return "Ngừng áp dụng";
            if (now < p.StartDate) return "Chưa bắt đầu";
            if (now > p.EndDate) return "Đã kết thúc";
            return "Đang diễn ra";
        }

        private static PromotionDto ToDto(Promotion p) => new PromotionDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            DiscountType = p.DiscountType,
            DiscountValue = p.DiscountValue,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            CurrentStatus = ComputeCurrentStatus(p)
        };

        public async Task<List<PromotionDto>> GetAllAsync()
        {
            var list = await _context.Promotions.OrderByDescending(p => p.StartDate).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<PromotionDto?> GetByIdAsync(int id)
        {
            var p = await _context.Promotions.FindAsync(id);
            return p == null ? null : ToDto(p);
        }

        public async Task<(PromotionDto? data, string? error)> CreateAsync(CreatePromotionDto dto)
        {
            if (dto.EndDate < dto.StartDate)
                return (null, "Ngày kết thúc phải sau ngày bắt đầu.");

            if (dto.DiscountType != "PhanTram" && dto.DiscountType != "SoTien")
                return (null, "Loại giảm giá không hợp lệ. Chỉ chấp nhận 'PhanTram' hoặc 'SoTien'.");

            if (dto.DiscountType == "PhanTram" && dto.DiscountValue > 100)
                return (null, "Giá trị giảm theo phần trăm không được vượt quá 100.");

            var codeExists = await _context.Promotions.AnyAsync(p => p.Code == dto.Code);
            if (codeExists) return (null, "Mã khuyến mãi đã tồn tại.");

            var promotion = new Promotion
            {
                Code = dto.Code,
                Name = dto.Name,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return (ToDto(promotion), null);
        }

        public async Task<(PromotionDto? data, string? error)> UpdateAsync(int id, UpdatePromotionDto dto)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return (null, "Không tìm thấy khuyến mãi.");

            if (dto.EndDate < dto.StartDate)
                return (null, "Ngày kết thúc phải sau ngày bắt đầu.");

            if (dto.DiscountType != "PhanTram" && dto.DiscountType != "SoTien")
                return (null, "Loại giảm giá không hợp lệ. Chỉ chấp nhận 'PhanTram' hoặc 'SoTien'.");

            if (dto.DiscountType == "PhanTram" && dto.DiscountValue > 100)
                return (null, "Giá trị giảm theo phần trăm không được vượt quá 100.");

            promotion.Name = dto.Name;
            promotion.DiscountType = dto.DiscountType;
            promotion.DiscountValue = dto.DiscountValue;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;
            promotion.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return (ToDto(promotion), null);
        }

        public async Task<(bool success, string? error)> DeleteAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return (false, "Không tìm thấy khuyến mãi.");

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}