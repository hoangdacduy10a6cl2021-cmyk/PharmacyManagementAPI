using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task<AlertsDto> GetAlertsAsync();
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate);
        Task<ProfitReportDto> GetProfitReportAsync(DateTime fromDate, DateTime toDate);
        Task<PaymentReportDto> GetPaymentReportAsync(DateTime fromDate, DateTime toDate);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private const int LOW_STOCK_THRESHOLD = 20;
        private const int EXPIRING_SOON_DAYS = 90;
        private const string CANCELLED = "Đã hủy";

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.Status != CANCELLED)
                .ToListAsync();

            var lowStockCount = await _context.Medicines
                .CountAsync(m => m.IsActive && m.Stock <= LOW_STOCK_THRESHOLD);

            var expiringThreshold = today.AddDays(EXPIRING_SOON_DAYS);
            var expiringSoonCount = await _context.Medicines
                .CountAsync(m => m.IsActive && m.ExpiryDate != null && m.ExpiryDate <= expiringThreshold);

            // Top 5 thuốc bán chạy nhất (không tính hoá đơn đã hủy)
            var topSelling = await _context.OrderDetails
                .Where(d => d.Order!.Status != CANCELLED)
                .Include(d => d.Medicine)
                .GroupBy(d => new { d.MedicineId, d.Medicine!.Name })
                .Select(g => new TopSellingMedicineDto
                {
                    MedicineId = g.Key.MedicineId,
                    MedicineName = g.Key.Name,
                    QuantitySold = g.Sum(d => d.Quantity)
                })
                .OrderByDescending(t => t.QuantitySold)
                .Take(5)
                .ToListAsync();

            // Doanh thu 7 ngày gần đây
            var sevenDaysAgo = today.AddDays(-6);
            var recentOrders = await _context.Orders
                .Where(o => o.OrderDate >= sevenDaysAgo && o.OrderDate < tomorrow && o.Status != CANCELLED)
                .ToListAsync();

            var revenueLast7Days = new List<DailyRevenueDto>();
            for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
            {
                var ordersOfDay = recentOrders.Where(o => o.OrderDate.Date == date).ToList();
                revenueLast7Days.Add(new DailyRevenueDto
                {
                    Date = date,
                    Revenue = ordersOfDay.Sum(o => o.FinalAmount),
                    OrderCount = ordersOfDay.Count
                });
            }

            return new DashboardDto
            {
                TodayRevenue = todayOrders.Sum(o => o.FinalAmount),
                TodayOrders = todayOrders.Count,
                LowStockCount = lowStockCount,
                ExpiringSoonCount = expiringSoonCount,
                TopSellingMedicines = topSelling,
                RevenueLast7Days = revenueLast7Days
            };
        }

        private static AlertMedicineDto ToAlert(Medicine m, DateTime today) => new AlertMedicineDto
        {
            Id = m.Id,
            Code = m.Code,
            Name = m.Name,
            Unit = m.Unit,
            Stock = m.Stock,
            ExpiryDate = m.ExpiryDate,
            DaysToExpire = m.ExpiryDate.HasValue ? (int)(m.ExpiryDate.Value.Date - today).TotalDays : null,
            ImageUrl = m.ImageUrl
        };

        public async Task<AlertsDto> GetAlertsAsync()
        {
            var today = DateTime.Now.Date;
            var soon = today.AddDays(EXPIRING_SOON_DAYS);

            var medicines = await _context.Medicines.Where(m => m.IsActive).ToListAsync();

            var lowStock = medicines.Where(m => m.Stock <= LOW_STOCK_THRESHOLD)
                .OrderBy(m => m.Stock).Select(m => ToAlert(m, today)).ToList();

            var expired = medicines.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date < today)
                .OrderBy(m => m.ExpiryDate).Select(m => ToAlert(m, today)).ToList();

            var expiring = medicines.Where(m => m.ExpiryDate.HasValue
                    && m.ExpiryDate.Value.Date >= today && m.ExpiryDate.Value.Date <= soon)
                .OrderBy(m => m.ExpiryDate).Select(m => ToAlert(m, today)).ToList();

            return new AlertsDto
            {
                LowStockCount = lowStock.Count,
                ExpiringSoonCount = expiring.Count,
                ExpiredCount = expired.Count,
                LowStock = lowStock,
                ExpiringSoon = expiring,
                Expired = expired
            };
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate)
        {
            var toDateInclusive = toDate.Date.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= fromDate.Date && o.OrderDate < toDateInclusive && o.Status != CANCELLED)
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.FinalAmount);
            var totalOrders = orders.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var dailyBreakdown = new List<DailyRevenueDto>();
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var ordersOfDay = orders.Where(o => o.OrderDate.Date == date).ToList();
                dailyBreakdown.Add(new DailyRevenueDto
                {
                    Date = date,
                    Revenue = ordersOfDay.Sum(o => o.FinalAmount),
                    OrderCount = ordersOfDay.Count
                });
            }

            return new RevenueReportDto
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = avgOrderValue,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<ProfitReportDto> GetProfitReportAsync(DateTime fromDate, DateTime toDate)
        {
            var toDateInclusive = toDate.Date.AddDays(1);

            var orders = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Medicine)
                .Where(o => o.OrderDate >= fromDate.Date && o.OrderDate < toDateInclusive && o.Status != CANCELLED)
                .ToListAsync();

            // Giá vốn = số lượng bán x giá nhập hiện tại của thuốc
            decimal CostOf(Order o) => o.OrderDetails.Sum(d => d.Quantity * (d.Medicine?.ImportPrice ?? 0));

            var totalRevenue = orders.Sum(o => o.FinalAmount);
            var totalCost = orders.Sum(CostOf);
            var totalProfit = totalRevenue - totalCost;

            var daily = new List<ProfitDailyDto>();
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var ofDay = orders.Where(o => o.OrderDate.Date == date).ToList();
                var rev = ofDay.Sum(o => o.FinalAmount);
                var cost = ofDay.Sum(CostOf);
                daily.Add(new ProfitDailyDto
                {
                    Date = date,
                    Revenue = rev,
                    Cost = cost,
                    Profit = rev - cost,
                    OrderCount = ofDay.Count
                });
            }

            // Top 10 thuốc lãi nhiều nhất (tính trên giá bán trước giảm giá)
            var top = orders.SelectMany(o => o.OrderDetails)
                .GroupBy(d => d.MedicineId)
                .Select(g =>
                {
                    var med = g.First().Medicine;
                    var rev = g.Sum(d => d.Subtotal);
                    var cost = g.Sum(d => d.Quantity * (d.Medicine?.ImportPrice ?? 0));
                    return new ProfitMedicineDto
                    {
                        MedicineId = g.Key,
                        MedicineName = med?.Name ?? "",
                        MedicineCode = med?.Code ?? "",
                        QuantitySold = g.Sum(d => d.Quantity),
                        Revenue = rev,
                        Cost = cost,
                        Profit = rev - cost
                    };
                })
                .OrderByDescending(t => t.Profit)
                .Take(10)
                .ToList();

            return new ProfitReportDto
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                TotalProfit = totalProfit,
                ProfitMargin = totalRevenue > 0 ? Math.Round(totalProfit / totalRevenue * 100, 1) : 0,
                TotalOrders = orders.Count,
                Daily = daily,
                TopMedicines = top
            };
        }

        public async Task<PaymentReportDto> GetPaymentReportAsync(DateTime fromDate, DateTime toDate)
        {
            var toDateInclusive = toDate.Date.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= fromDate.Date && o.OrderDate < toDateInclusive && o.Status != CANCELLED)
                .ToListAsync();

            var total = orders.Sum(o => o.FinalAmount);

            var methods = orders.GroupBy(o => o.PaymentMethod)
                .Select(g => new PaymentMethodStatDto
                {
                    Method = g.Key,
                    OrderCount = g.Count(),
                    Amount = g.Sum(o => o.FinalAmount),
                    Percent = total > 0 ? Math.Round(g.Sum(o => o.FinalAmount) / total * 100, 1) : 0
                })
                .OrderByDescending(m => m.Amount)
                .ToList();

            return new PaymentReportDto
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                TotalAmount = total,
                TotalOrders = orders.Count,
                Methods = methods
            };
        }
    }
}