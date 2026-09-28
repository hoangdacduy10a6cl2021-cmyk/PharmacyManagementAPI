namespace PharmacyManagementWeb.Models.ApiModels
{
    public class RevenueReportModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueModel> DailyBreakdown { get; set; } = new();
    }
}