using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllAsync(int? customerId, DateTime? fromDate, DateTime? toDate);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<(OrderDto? data, string? error)> CreateAsync(CreateOrderDto dto, int userId);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        // Khách chi tiêu từ mức này trở lên -> tự động nâng hạng VIP
        private const decimal VIP_THRESHOLD = 5_000_000;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static OrderDto ToDto(Order o) => new OrderDto
        {
            Id = o.Id,
            Code = o.Code,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer?.Name,
            UserId = o.UserId,
            UserName = o.User?.FullName,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            DiscountAmount = o.DiscountAmount,
            FinalAmount = o.FinalAmount,
            PaymentMethod = o.PaymentMethod,
            Status = o.Status,
            Details = o.OrderDetails.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                MedicineId = d.MedicineId,
                MedicineName = d.Medicine?.Name,
                MedicineCode = d.Medicine?.Code,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        };

        public async Task<List<OrderDto>> GetAllAsync(int? customerId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Medicine)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            var list = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var o = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(o => o.Id == id);

            return o == null ? null : ToDto(o);
        }

        public async Task<(OrderDto? data, string? error)> CreateAsync(CreateOrderDto dto, int userId)
        {
            if (dto.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId.Value);
                if (!customerExists) return (null, "Khách hàng không tồn tại.");
            }

            var medicineIds = dto.Details.Select(d => d.MedicineId).Distinct().ToList();
            var medicines = await _context.Medicines
                .Where(m => medicineIds.Contains(m.Id))
                .ToListAsync();

            if (medicines.Count != medicineIds.Count)
                return (null, "Có thuốc trong hoá đơn không tồn tại.");

            // Kiểm tra thuốc còn kinh doanh và đủ tồn kho trước khi trừ
            foreach (var item in dto.Details)
            {
                var medicine = medicines.First(m => m.Id == item.MedicineId);

                if (!medicine.IsActive)
                    return (null, $"Thuốc '{medicine.Name}' đã ngưng kinh doanh.");

                if (medicine.Stock < item.Quantity)
                    return (null, $"Thuốc '{medicine.Name}' không đủ tồn kho (còn {medicine.Stock}, cần {item.Quantity}).");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lastId = await _context.Orders.CountAsync();
                var code = $"HD{(lastId + 1):D4}"; // HD0001, HD0002...

                var totalAmount = dto.Details.Sum(d => d.Quantity * medicines.First(m => m.Id == d.MedicineId).SellPrice);
                var finalAmount = totalAmount - dto.DiscountAmount;
                if (finalAmount < 0) finalAmount = 0;

                var order = new Order
                {
                    Code = code,
                    CustomerId = dto.CustomerId,
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    DiscountAmount = dto.DiscountAmount,
                    FinalAmount = finalAmount,
                    PaymentMethod = dto.PaymentMethod,
                    Status = "Hoàn thành"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in dto.Details)
                {
                    var medicine = medicines.First(m => m.Id == item.MedicineId);

                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        MedicineId = item.MedicineId,
                        Quantity = item.Quantity,
                        UnitPrice = medicine.SellPrice,
                        Subtotal = item.Quantity * medicine.SellPrice
                    });

                    // Tự động trừ tồn kho
                    medicine.Stock -= item.Quantity;
                }

                // Cộng dồn chi tiêu và tự động nâng hạng VIP cho khách hàng (nếu có chọn khách hàng)
                if (dto.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(dto.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.TotalSpent += finalAmount;
                        if (customer.TotalSpent >= VIP_THRESHOLD && customer.CustomerType != "VIP")
                            customer.CustomerType = "VIP";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await GetByIdAsync(order.Id);
                return (result, null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Có lỗi xảy ra khi tạo hoá đơn.");
            }
        }
    }
}