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
        Task<(OrderDto? data, string? error)> CreateAsync(CreateOrderDto dto, int userId, bool isAdmin);
        Task<(OrderDto? data, string? error)> CancelAsync(int id);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        private const string CANCELLED = "Đã hủy";

        // Khách chi tiêu từ mức này trở lên -> tự động nâng hạng VIP
        private const decimal VIP_THRESHOLD = 5_000_000;

        // Nhân viên chỉ được giảm tối đa X% giá trị đơn, Admin không bị giới hạn (chỉnh số này nếu cần)
        private const decimal MAX_STAFF_DISCOUNT_PERCENT = 10;

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
                MedicineImageUrl = d.Medicine?.ImageUrl,
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

        public async Task<(OrderDto? data, string? error)> CreateAsync(CreateOrderDto dto, int userId, bool isAdmin)
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

                if (medicine.ExpiryDate.HasValue && medicine.ExpiryDate.Value.Date < DateTime.Today)
                    return (null, $"Thuốc '{medicine.Name}' đã hết hạn sử dụng ({medicine.ExpiryDate.Value:dd/MM/yyyy}), không thể bán.");

                if (medicine.Stock < item.Quantity)
                    return (null, $"Thuốc '{medicine.Name}' không đủ tồn kho (còn {medicine.Stock}, cần {item.Quantity}).");
            }

            var totalAmount = dto.Details.Sum(d => d.Quantity * medicines.First(m => m.Id == d.MedicineId).SellPrice);

            // Kiểm soát giảm giá
            if (dto.DiscountAmount > totalAmount)
                return (null, $"Giảm giá ({dto.DiscountAmount:N0}đ) không được vượt quá tạm tính ({totalAmount:N0}đ).");

            if (!isAdmin)
            {
                var maxDiscount = totalAmount * MAX_STAFF_DISCOUNT_PERCENT / 100;
                if (dto.DiscountAmount > maxDiscount)
                    return (null, $"Nhân viên chỉ được giảm tối đa {MAX_STAFF_DISCOUNT_PERCENT}% ({maxDiscount:N0}đ) cho đơn này. Giảm nhiều hơn cần Admin thực hiện.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lastId = await _context.Orders.CountAsync();
                var code = $"HD{(lastId + 1):D4}"; // HD0001, HD0002...

                var finalAmount = totalAmount - dto.DiscountAmount;

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

        // Hủy hoá đơn = hoàn tiền: trả thuốc về kho, trừ lại chi tiêu của khách, đổi trạng thái "Đã hủy"
        public async Task<(OrderDto? data, string? error)> CancelAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return (null, "Không tìm thấy hoá đơn.");
            if (order.Status == CANCELLED) return (null, "Hoá đơn này đã được hủy trước đó.");

            var hasReturns = await _context.ReturnOrders.AnyAsync(r => r.OrderId == id && r.Status != "Từ chối");
            if (hasReturns) return (null, "Hoá đơn này đã có phiếu trả hàng nên không thể hủy.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in order.OrderDetails)
                {
                    var medicine = await _context.Medicines.FindAsync(detail.MedicineId);
                    if (medicine != null) medicine.Stock += detail.Quantity;
                }

                if (order.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(order.CustomerId.Value);
                    if (customer != null)
                        customer.TotalSpent = Math.Max(0, customer.TotalSpent - order.FinalAmount);
                }

                order.Status = CANCELLED;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetByIdAsync(id), null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Có lỗi xảy ra khi hủy hoá đơn.");
            }
        }
    }
}