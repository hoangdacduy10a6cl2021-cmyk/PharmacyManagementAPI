using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDetailModel
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class OrderModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OrderDetailModel> Details { get; set; } = new();
    }

    // Dùng để deserialize giỏ hàng (JSON) do JavaScript gửi lên khi submit form Thanh toán
    public class CartItemInput
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}