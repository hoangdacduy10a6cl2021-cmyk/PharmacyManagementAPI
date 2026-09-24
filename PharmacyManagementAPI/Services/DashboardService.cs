using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;

namespace PharmacyManagementAPI.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private const int LOW_STOCK_THRESHOLD = 20;
        private const int EXPIRING_SOON_DAYS = 90;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .ToListAsync();

            var lowStockCount = await _context.Medicines
                .CountAsync(m => m.IsActive && m.Stock <= LOW_STOCK_THRESHOLD);

            var expiringThreshold = today.AddDays(EXPIRING_SOON_DAYS);
            var expiringSoonCount = await _context.Medicines
                .CountAsync(m => m.IsActive && m.ExpiryDate != null && m.ExpiryDate <= expiringThreshold);

            // Top 5 thuốc bán chạy nhất (tính trên toàn bộ lịch sử đơn hàng)
            var topSelling = await _context.OrderDetails
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
                .Where(o => o.OrderDate >= sevenDaysAgo && o.OrderDate < tomorrow)
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

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate)
        {
            var toDateInclusive = toDate.Date.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= fromDate.Date && o.OrderDate < toDateInclusive)
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
    }
}