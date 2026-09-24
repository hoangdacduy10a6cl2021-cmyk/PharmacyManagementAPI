namespace PharmacyManagementWeb.Models.ApiModels
{
    public class TopSellingMedicineModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
    }

    public class DailyRevenueModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class DashboardModel
    {
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public List<TopSellingMedicineModel> TopSellingMedicines { get; set; } = new();
        public List<DailyRevenueModel> RevenueLast7Days { get; set; } = new();
    }
}