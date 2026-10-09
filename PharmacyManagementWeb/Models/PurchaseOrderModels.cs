namespace PharmacyManagementWeb.Models.ApiModels
{
    public class PurchaseOrderDetailModel
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineCode { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PurchaseOrderModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<PurchaseOrderDetailModel> Details { get; set; } = new();
    }

    // Dùng để deserialize giỏ hàng nhập (JSON) do JavaScript gửi lên
    public class PurchaseOrderCartItemInput
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}