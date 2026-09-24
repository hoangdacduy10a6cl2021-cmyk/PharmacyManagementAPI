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
}