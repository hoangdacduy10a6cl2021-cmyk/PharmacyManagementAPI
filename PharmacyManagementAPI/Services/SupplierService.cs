using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();
        Task<SupplierDto?> GetByIdAsync(int id);
        Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
        Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto);
        Task<(bool success, string? error)> DeleteAsync(int id);
    }

    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static SupplierDto ToDto(Supplier s) => new SupplierDto
        {
            Id = s.Id,
            Name = s.Name,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        };

        public async Task<List<SupplierDto>> GetAllAsync()
        {
            return await _context.Suppliers.Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToListAsync();
        }

        public async Task<SupplierDto?> GetByIdAsync(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            return s == null ? null : ToDto(s);
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                IsActive = true
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return ToDto(supplier);
        }

        public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return null;

            supplier.Name = dto.Name;
            supplier.Phone = dto.Phone;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;
            supplier.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return ToDto(supplier);
        }

        public async Task<(bool success, string? error)> DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return (false, "Không tìm thấy nhà cung cấp.");

            var hasMedicines = await _context.Medicines.AnyAsync(m => m.SupplierId == id);
            var hasPurchaseOrders = await _context.PurchaseOrders.AnyAsync(p => p.SupplierId == id);

            if (hasMedicines || hasPurchaseOrders)
            {
                // Không xoá cứng vì đã có dữ liệu liên kết -> chuyển sang ngưng hoạt động
                supplier.IsActive = false;
                await _context.SaveChangesAsync();
                return (true, "Nhà cung cấp đang có dữ liệu liên kết nên đã được chuyển sang trạng thái ngưng hoạt động thay vì xoá.");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}