namespace PharmacyManagementAPI.Models.DTOs
{
    public class TopSellingMedicineDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class DashboardDto
    {
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public List<TopSellingMedicineDto> TopSellingMedicines { get; set; } = new();
        public List<DailyRevenueDto> RevenueLast7Days { get; set; } = new();
    }

    public class RevenueReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
    }

    // ===== Cảnh báo tồn kho / hạn dùng =====
    public class AlertMedicineDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int Stock { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DaysToExpire { get; set; }
        public string? ImageUrl { get; set; }
        public string? BatchNumber { get; set; }
        public int? BatchId { get; set; }
    }

    public class AlertsDto
    {
        public int LowStockCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int ExpiredCount { get; set; }
        public int LowStockThreshold { get; set; }
        public int ExpiringSoonDays { get; set; }
        public List<AlertMedicineDto> LowStock { get; set; } = new();
        public List<AlertMedicineDto> ExpiringSoon { get; set; } = new();
        public List<AlertMedicineDto> Expired { get; set; } = new();
    }

    // ===== Báo cáo lợi nhuận =====
    public class ProfitDailyDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public int OrderCount { get; set; }
    }

    public class ProfitMedicineDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
    }

    public class ProfitReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalOrders { get; set; }
        public List<ProfitDailyDto> Daily { get; set; } = new();
        public List<ProfitMedicineDto> TopMedicines { get; set; } = new();
    }

    // ===== Thống kê thanh toán =====
    public class PaymentMethodStatDto
    {
        public string Method { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Amount { get; set; }
        public decimal Percent { get; set; }
    }

    public class PaymentReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalOrders { get; set; }
        public List<PaymentMethodStatDto> Methods { get; set; } = new();
    }
}