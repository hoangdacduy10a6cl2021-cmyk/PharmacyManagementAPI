using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IStoreSettingService
    {
        Task<StoreSetting> GetEntityAsync();
        Task<StoreSettingDto> GetAsync();
        Task<(StoreSettingDto? data, string? error)> UpdateAsync(UpdateStoreSettingDto dto);
    }

    public class StoreSettingService : IStoreSettingService
    {
        private readonly ApplicationDbContext _context;

        public StoreSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy cài đặt, nếu chưa có thì tự tạo dòng mặc định
        public async Task<StoreSetting> GetEntityAsync()
        {
            var setting = await _context.StoreSettings.OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new StoreSetting();
                _context.StoreSettings.Add(setting);
                await _context.SaveChangesAsync();
            }
            return setting;
        }

        private static StoreSettingDto ToDto(StoreSetting s) => new StoreSettingDto
        {
            StoreName = s.StoreName,
            Address = s.Address,
            Phone = s.Phone,
            Email = s.Email,
            TaxCode = s.TaxCode,
            LowStockThreshold = s.LowStockThreshold,
            ExpiringSoonDays = s.ExpiringSoonDays,
            MaxStaffDiscountPercent = s.MaxStaffDiscountPercent,
            ReceiptFooter = s.ReceiptFooter
        };

        public async Task<StoreSettingDto> GetAsync()
        {
            return ToDto(await GetEntityAsync());
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public async Task<(StoreSettingDto? data, string? error)> UpdateAsync(UpdateStoreSettingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.StoreName))
                return (null, "Tên cửa hàng không được để trống.");

            var s = await GetEntityAsync();

            s.StoreName = dto.StoreName.Trim();
            s.Address = Clean(dto.Address);
            s.Phone = Clean(dto.Phone);
            s.Email = Clean(dto.Email);
            s.TaxCode = Clean(dto.TaxCode);
            s.LowStockThreshold = dto.LowStockThreshold;
            s.ExpiringSoonDays = dto.ExpiringSoonDays;
            s.MaxStaffDiscountPercent = dto.MaxStaffDiscountPercent;
            s.ReceiptFooter = Clean(dto.ReceiptFooter);
            s.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return (ToDto(s), null);
        }
    }
}