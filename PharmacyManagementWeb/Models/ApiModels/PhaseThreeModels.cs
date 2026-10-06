namespace PharmacyManagementWeb.Models.ApiModels
{
    // ===== Trả hàng =====
    public class ReturnableItemModel
    {
        public int OrderDetailId { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int SoldQuantity { get; set; }
        public int ReturnedQuantity { get; set; }
        public int MaxReturnQuantity { get; set; }
    }

    public class ReturnableOrderModel
    {
        public int OrderId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string? CustomerName { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<ReturnableItemModel> Items { get; set; } = new();
    }

    public class ReturnDetailModel
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ReturnOrderModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
        public bool RestockItems { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
        public List<ReturnDetailModel> Details { get; set; } = new();
    }

    // Dùng để đọc danh sách thuốc trả (JSON) do JavaScript gửi lên
    public class ReturnItemInput
    {
        public int OrderDetailId { get; set; }
        public int Quantity { get; set; }
    }

    // ===== Kiểm kê kho =====
    public class StockCheckDetailModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public int SystemQty { get; set; }
        public int ActualQty { get; set; }
        public int Difference { get; set; }
        public string? Note { get; set; }
    }

    public class StockCheckModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
        public string? Note { get; set; }
        public int ItemCount { get; set; }
        public int DifferenceCount { get; set; }
        public int SurplusQty { get; set; }
        public int ShortageQty { get; set; }
        public decimal ValueDifference { get; set; }
        public List<StockCheckDetailModel> Details { get; set; } = new();
    }

    // Dùng để đọc số đếm (JSON) do JavaScript gửi lên
    public class StockCheckItemInput
    {
        public int MedicineId { get; set; }
        public int ActualQty { get; set; }
    }
}