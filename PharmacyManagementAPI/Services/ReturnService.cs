using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IReturnService
    {
        Task<List<ReturnOrderDto>> GetAllAsync(string? status);
        Task<ReturnOrderDto?> GetByIdAsync(int id);
        Task<(ReturnableOrderDto? data, string? error)> LookupAsync(string code);
        Task<(ReturnOrderDto? data, string? error)> CreateAsync(CreateReturnDto dto, int userId, bool isAdmin);
        Task<(ReturnOrderDto? data, string? error)> ApproveAsync(int id, int adminId);
        Task<(ReturnOrderDto? data, string? error)> RejectAsync(int id, int adminId, string? note);
    }

    public class ReturnService : IReturnService
    {
        private readonly ApplicationDbContext _context;

        private const string PENDING = "Chờ duyệt";
        private const string APPROVED = "Đã duyệt";
        private const string REJECTED = "Từ chối";
        private const string ORDER_CANCELLED = "Đã hủy";

        public ReturnService(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<ReturnOrder> Query() => _context.ReturnOrders
            .Include(r => r.Order)
            .Include(r => r.Customer)
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.ReturnDetails).ThenInclude(d => d.Medicine);

        private static ReturnOrderDto ToDto(ReturnOrder r) => new ReturnOrderDto
        {
            Id = r.Id,
            Code = r.Code,
            OrderId = r.OrderId,
            OrderCode = r.Order?.Code ?? "",
            CustomerName = r.Customer?.Name,
            CreatedByName = r.CreatedBy?.FullName,
            CreatedAt = r.CreatedAt,
            Reason = r.Reason,
            Note = r.Note,
            RestockItems = r.RestockItems,
            RefundAmount = r.RefundAmount,
            RefundMethod = r.RefundMethod,
            Status = r.Status,
            ReviewedByName = r.ReviewedBy?.FullName,
            ReviewedAt = r.ReviewedAt,
            ReviewNote = r.ReviewNote,
            Details = r.ReturnDetails.Select(d => new ReturnDetailDto
            {
                Id = d.Id,
                MedicineId = d.MedicineId,
                MedicineName = d.Medicine?.Name ?? "",
                MedicineCode = d.Medicine?.Code ?? "",
                ImageUrl = d.Medicine?.ImageUrl,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal
            }).ToList()
        };

        public async Task<List<ReturnOrderDto>> GetAllAsync(string? status)
        {
            var query = Query();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<ReturnOrderDto?> GetByIdAsync(int id)
        {
            var r = await Query().FirstOrDefaultAsync(x => x.Id == id);
            return r == null ? null : ToDto(r);
        }

        // Số lượng đã trả (phiếu chờ duyệt + đã duyệt) theo từng dòng hoá đơn
        private async Task<Dictionary<int, int>> GetReturnedMapAsync(int orderId)
        {
            return await _context.ReturnDetails
                .Where(d => d.ReturnOrder!.OrderId == orderId && d.ReturnOrder.Status != REJECTED)
                .GroupBy(d => d.OrderDetailId)
                .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.Key, x => x.Qty);
        }

        public async Task<(ReturnableOrderDto? data, string? error)> LookupAsync(string code)
        {
            var normalized = (code ?? "").Trim().ToUpper();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(o => o.Code == normalized);

            if (order == null) return (null, $"Không tìm thấy hoá đơn có mã '{normalized}'.");
            if (order.Status == ORDER_CANCELLED) return (null, "Hoá đơn này đã bị hủy nên không thể trả hàng.");

            var returned = await GetReturnedMapAsync(order.Id);

            var items = order.OrderDetails.Select(d =>
            {
                var done = returned.TryGetValue(d.Id, out var q) ? q : 0;
                return new ReturnableItemDto
                {
                    OrderDetailId = d.Id,
                    MedicineId = d.MedicineId,
                    MedicineName = d.Medicine?.Name ?? "",
                    MedicineCode = d.Medicine?.Code ?? "",
                    ImageUrl = d.Medicine?.ImageUrl,
                    UnitPrice = d.UnitPrice,
                    SoldQuantity = d.Quantity,
                    ReturnedQuantity = done,
                    MaxReturnQuantity = Math.Max(0, d.Quantity - done)
                };
            }).ToList();

            return (new ReturnableOrderDto
            {
                OrderId = order.Id,
                Code = order.Code,
                OrderDate = order.OrderDate,
                CustomerName = order.Customer?.Name,
                PaymentMethod = order.PaymentMethod,
                TotalAmount = order.TotalAmount,
                FinalAmount = order.FinalAmount,
                Items = items
            }, null);
        }

        public async Task<(ReturnOrderDto? data, string? error)> CreateAsync(CreateReturnDto dto, int userId, bool isAdmin)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null) return (null, "Không tìm thấy hoá đơn.");
            if (order.Status == ORDER_CANCELLED) return (null, "Hoá đơn này đã bị hủy nên không thể trả hàng.");

            var returned = await GetReturnedMapAsync(order.Id);

            // Gộp các dòng trùng, kiểm tra số lượng tối đa được trả
            var lines = new List<(OrderDetail detail, int qty)>();
            foreach (var g in dto.Items.GroupBy(i => i.OrderDetailId))
            {
                var detail = order.OrderDetails.FirstOrDefault(d => d.Id == g.Key);
                if (detail == null) return (null, "Có thuốc không thuộc hoá đơn này.");

                var qty = g.Sum(x => x.Quantity);
                var done = returned.TryGetValue(detail.Id, out var q) ? q : 0;
                var max = detail.Quantity - done;
                if (qty > max)
                    return (null, $"Số lượng trả vượt quá số được phép (tối đa {max}) cho một dòng thuốc trong hoá đơn.");

                lines.Add((detail, qty));
            }

            if (!lines.Any()) return (null, "Phải chọn ít nhất 1 thuốc để trả.");

            // Tiền hoàn = giá trị hàng trả, nhân tỷ lệ giảm giá của hoá đơn gốc
            var subtotal = lines.Sum(l => l.qty * l.detail.UnitPrice);
            var ratio = order.TotalAmount > 0 ? order.FinalAmount / order.TotalAmount : 1m;
            var refund = Math.Round(subtotal * ratio, 0);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var count = await _context.ReturnOrders.CountAsync();

                var ret = new ReturnOrder
                {
                    Code = $"TH{(count + 1):D4}",
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now,
                    Reason = dto.Reason,
                    Note = dto.Note,
                    RestockItems = dto.RestockItems,
                    RefundAmount = refund,
                    RefundMethod = string.IsNullOrWhiteSpace(dto.RefundMethod) ? "Tiền mặt" : dto.RefundMethod,
                    Status = PENDING
                };

                foreach (var (detail, qty) in lines)
                {
                    ret.ReturnDetails.Add(new ReturnDetail
                    {
                        OrderDetailId = detail.Id,
                        MedicineId = detail.MedicineId,
                        Quantity = qty,
                        UnitPrice = detail.UnitPrice,
                        Subtotal = qty * detail.UnitPrice
                    });
                }

                _context.ReturnOrders.Add(ret);
                await _context.SaveChangesAsync();

                // Admin tạo phiếu thì được duyệt luôn
                if (isAdmin)
                {
                    await ApplyApprovalAsync(ret, userId, null);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (await GetByIdAsync(ret.Id), null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Có lỗi xảy ra khi tạo phiếu trả hàng.");
            }
        }

        // Áp dụng khi duyệt: nhập lại kho (nếu chọn), trừ chi tiêu của khách, chuyển trạng thái
        private async Task ApplyApprovalAsync(ReturnOrder ret, int reviewerId, string? note)
        {
            if (ret.RestockItems)
            {
                foreach (var d in ret.ReturnDetails)
                {
                    var medicine = await _context.Medicines.FindAsync(d.MedicineId);
                    if (medicine != null) medicine.Stock += d.Quantity;
                }
            }

            if (ret.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(ret.CustomerId.Value);
                if (customer != null)
                    customer.TotalSpent = Math.Max(0, customer.TotalSpent - ret.RefundAmount);
            }

            ret.Status = APPROVED;
            ret.ReviewedByUserId = reviewerId;
            ret.ReviewedAt = DateTime.Now;
            ret.ReviewNote = note;
        }

        public async Task<(ReturnOrderDto? data, string? error)> ApproveAsync(int id, int adminId)
        {
            var ret = await _context.ReturnOrders
                .Include(r => r.Order)
                .Include(r => r.ReturnDetails)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (ret == null) return (null, "Không tìm thấy phiếu trả hàng.");
            if (ret.Status != PENDING) return (null, "Phiếu này đã được xử lý trước đó.");
            if (ret.Order?.Status == ORDER_CANCELLED) return (null, "Hoá đơn gốc đã bị hủy, không thể duyệt phiếu này.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await ApplyApprovalAsync(ret, adminId, null);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (await GetByIdAsync(id), null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Có lỗi xảy ra khi duyệt phiếu trả hàng.");
            }
        }

        public async Task<(ReturnOrderDto? data, string? error)> RejectAsync(int id, int adminId, string? note)
        {
            var ret = await _context.ReturnOrders.FirstOrDefaultAsync(r => r.Id == id);
            if (ret == null) return (null, "Không tìm thấy phiếu trả hàng.");
            if (ret.Status != PENDING) return (null, "Phiếu này đã được xử lý trước đó.");

            ret.Status = REJECTED;
            ret.ReviewedByUserId = adminId;
            ret.ReviewedAt = DateTime.Now;
            ret.ReviewNote = note;

            await _context.SaveChangesAsync();
            return (await GetByIdAsync(id), null);
        }
    }
}