using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IMedicineService
    {
        Task<List<MedicineDto>> GetAllAsync(string? search, int? categoryId, bool? lowStock, bool? expiringSoon);
        Task<MedicineDto?> GetByIdAsync(int id);
        Task<MedicineDto?> GetByBarcodeAsync(string barcode);
        Task<(MedicineDto? data, string? error)> CreateAsync(CreateMedicineDto dto);
        Task<(MedicineDto? data, string? error)> UpdateAsync(int id, UpdateMedicineDto dto);
        Task<(bool success, string? error)> DeleteAsync(int id);
    }

    public class MedicineService : IMedicineService
    {
        private readonly ApplicationDbContext _context;
        private const int LOW_STOCK_THRESHOLD = 20;
        private const int EXPIRING_SOON_DAYS = 90;

        public MedicineService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static MedicineDto ToDto(Medicine m) => new MedicineDto
        {
            Id = m.Id,
            Code = m.Code,
            Name = m.Name,
            CategoryId = m.CategoryId,
            CategoryName = m.Category?.Name,
            SupplierId = m.SupplierId,
            SupplierName = m.Supplier?.Name,
            Unit = m.Unit,
            SellPrice = m.SellPrice,
            ImportPrice = m.ImportPrice,
            Stock = m.Stock,
            ExpiryDate = m.ExpiryDate,
            Barcode = m.Barcode,
            IsActive = m.IsActive
        };

        public async Task<List<MedicineDto>> GetAllAsync(string? search, int? categoryId, bool? lowStock, bool? expiringSoon)
        {
            var query = _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(search)
                    || m.Code.ToLower().Contains(search)
                    || (m.Barcode != null && m.Barcode.ToLower().Contains(search)));
            }

            if (categoryId.HasValue)
                query = query.Where(m => m.CategoryId == categoryId.Value);

            if (lowStock == true)
                query = query.Where(m => m.Stock <= LOW_STOCK_THRESHOLD);

            if (expiringSoon == true)
            {
                var threshold = DateTime.Now.AddDays(EXPIRING_SOON_DAYS);
                query = query.Where(m => m.ExpiryDate != null && m.ExpiryDate <= threshold);
            }

            var list = await query.OrderBy(m => m.Name).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<MedicineDto?> GetByIdAsync(int id)
        {
            var m = await _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            return m == null ? null : ToDto(m);
        }

        public async Task<MedicineDto?> GetByBarcodeAsync(string barcode)
        {
            var m = await _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Barcode == barcode);

            return m == null ? null : ToDto(m);
        }

        public async Task<(MedicineDto? data, string? error)> CreateAsync(CreateMedicineDto dto)
        {
            var codeExists = await _context.Medicines.AnyAsync(m => m.Code == dto.Code);
            if (codeExists) return (null, "Mã thuốc đã tồn tại.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return (null, "Danh mục không tồn tại.");

            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value);
                if (!supplierExists) return (null, "Nhà cung cấp không tồn tại.");
            }

            var medicine = new Medicine
            {
                Code = dto.Code,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId,
                Unit = dto.Unit,
                SellPrice = dto.SellPrice,
                ImportPrice = dto.ImportPrice,
                Stock = dto.Stock,
                ExpiryDate = dto.ExpiryDate,
                Barcode = dto.Barcode,
                IsActive = true
            };

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            await _context.Entry(medicine).Reference(m => m.Category).LoadAsync();
            if (medicine.SupplierId.HasValue)
                await _context.Entry(medicine).Reference(m => m.Supplier).LoadAsync();

            return (ToDto(medicine), null);
        }

        public async Task<(MedicineDto? data, string? error)> UpdateAsync(int id, UpdateMedicineDto dto)
        {
            var medicine = await _context.Medicines
                .Include(m => m.Category)
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null) return (null, "Không tìm thấy thuốc.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return (null, "Danh mục không tồn tại.");

            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value);
                if (!supplierExists) return (null, "Nhà cung cấp không tồn tại.");
            }

            medicine.Name = dto.Name;
            medicine.CategoryId = dto.CategoryId;
            medicine.SupplierId = dto.SupplierId;
            medicine.Unit = dto.Unit;
            medicine.SellPrice = dto.SellPrice;
            medicine.ImportPrice = dto.ImportPrice;
            medicine.ExpiryDate = dto.ExpiryDate;
            medicine.Barcode = dto.Barcode;
            medicine.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            await _context.Entry(medicine).Reference(m => m.Category).LoadAsync();
            if (medicine.SupplierId.HasValue)
                await _context.Entry(medicine).Reference(m => m.Supplier).LoadAsync();
            else
                medicine.Supplier = null;

            return (ToDto(medicine), null);
        }

        public async Task<(bool success, string? error)> DeleteAsync(int id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return (false, "Không tìm thấy thuốc.");

            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.MedicineId == id);
            var hasPurchase = await _context.PurchaseOrderDetails.AnyAsync(pd => pd.MedicineId == id);
            var hasPrescriptions = await _context.PrescriptionDetails.AnyAsync(pd => pd.MedicineId == id);

            if (hasOrders || hasPurchase || hasPrescriptions)
            {
                // Đã có giao dịch liên quan -> ngưng kinh doanh thay vì xoá cứng
                medicine.IsActive = false;
                await _context.SaveChangesAsync();
                return (true, "Thuốc đã có giao dịch liên kết nên đã được chuyển sang trạng thái ngưng kinh doanh thay vì xoá.");
            }

            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}