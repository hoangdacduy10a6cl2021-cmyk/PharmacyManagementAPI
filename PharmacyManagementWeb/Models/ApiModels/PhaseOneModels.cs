namespace PharmacyManagementWeb.Models.ApiModels
{
    // ===== Cảnh báo =====
    public class AlertMedicineModel
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

    public class AlertsModel
    {
        public int LowStockCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int ExpiredCount { get; set; }
        public int LowStockThreshold { get; set; } = 20;
        public int ExpiringSoonDays { get; set; } = 90;
        public List<AlertMedicineModel> LowStock { get; set; } = new();
        public List<AlertMedicineModel> ExpiringSoon { get; set; } = new();
        public List<AlertMedicineModel> Expired { get; set; } = new();
    }

    // ===== Lợi nhuận =====
    public class ProfitDailyModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public int OrderCount { get; set; }
    }

    public class ProfitMedicineModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
    }

    public class ProfitReportModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalOrders { get; set; }
        public List<ProfitDailyModel> Daily { get; set; } = new();
        public List<ProfitMedicineModel> TopMedicines { get; set; } = new();
    }

    // ===== Thanh toán =====
    public class PaymentMethodStatModel
    {
        public string Method { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Amount { get; set; }
        public decimal Percent { get; set; }
    }

    public class PaymentReportModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalOrders { get; set; }
        public List<PaymentMethodStatModel> Methods { get; set; } = new();
    }

    // ===== Tài khoản cá nhân =====
    public class ProfileModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}