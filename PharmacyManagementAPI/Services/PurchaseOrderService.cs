using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrderDto>> GetAllAsync(int? supplierId, DateTime? fromDate, DateTime? toDate);
        Task<PurchaseOrderDto?> GetByIdAsync(int id);
        Task<(PurchaseOrderDto? data, string? error)> CreateAsync(CreatePurchaseOrderDto dto, int userId);
    }

    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static PurchaseOrderDto ToDto(PurchaseOrder p) => new PurchaseOrderDto
        {
            Id = p.Id,
            Code = p.Code,
            SupplierId = p.SupplierId,
            SupplierName = p.Supplier?.Name,
            UserId = p.UserId,
            UserName = p.User?.FullName,
            ImportDate = p.ImportDate,
            TotalAmount = p.TotalAmount,
            Status = p.Status,
            Details = p.PurchaseOrderDetails.Select(d => new PurchaseOrderDetailDto
            {
                Id = d.Id,
                MedicineId = d.MedicineId,
                MedicineName = d.Medicine?.Name,
                MedicineCode = d.Medicine?.Code,
                Quantity = d.Quantity,
                ImportPrice = d.ImportPrice
            }).ToList()
        };

        public async Task<List<PurchaseOrderDto>> GetAllAsync(int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseOrderDetails).ThenInclude(d => d.Medicine)
                .AsQueryable();

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.ImportDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.ImportDate <= toDate.Value);

            var list = await query.OrderByDescending(p => p.ImportDate).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
        {
            var p = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseOrderDetails).ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);

            return p == null ? null : ToDto(p);
        }

        public async Task<(PurchaseOrderDto? data, string? error)> CreateAsync(CreatePurchaseOrderDto dto, int userId)
        {
            var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null) return (null, "Nhà cung cấp không tồn tại.");

            var medicineIds = dto.Details.Select(d => d.MedicineId).Distinct().ToList();
            var medicines = await _context.Medicines
                .Where(m => medicineIds.Contains(m.Id))
                .ToListAsync();

            if (medicines.Count != medicineIds.Count)
                return (null, "Có thuốc trong phiếu nhập không tồn tại.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lastId = await _context.PurchaseOrders.CountAsync();
                var code = $"PN{(lastId + 1):D4}"; // PN0001, PN0002...

                var purchaseOrder = new PurchaseOrder
                {
                    Code = code,
                    SupplierId = dto.SupplierId,
                    UserId = userId,
                    ImportDate = DateTime.Now,
                    Status = "Đã nhập",
                    TotalAmount = dto.Details.Sum(d => d.Quantity * d.ImportPrice)
                };

                _context.PurchaseOrders.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                foreach (var item in dto.Details)
                {
                    _context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        MedicineId = item.MedicineId,
                        Quantity = item.Quantity,
                        ImportPrice = item.ImportPrice
                    });

                    // Tự động cộng tồn kho + cập nhật giá nhập mới nhất
                    var medicine = medicines.First(m => m.Id == item.MedicineId);
                    medicine.Stock += item.Quantity;
                    medicine.ImportPrice = item.ImportPrice;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await GetByIdAsync(purchaseOrder.Id);
                return (result, null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Có lỗi xảy ra khi tạo phiếu nhập.");
            }
        }
    }
}