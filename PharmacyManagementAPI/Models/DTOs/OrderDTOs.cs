using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class OrderDto
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
        public List<OrderDetailDto> Details { get; set; } = new();
    }

    public class CreateOrderDetailDto
    {
        [Required]
        public int MedicineId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }

    public class CreateOrderDto
    {
        // Không bắt buộc - khách vãng lai không cần chọn khách hàng
        public int? CustomerId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Hoá đơn phải có ít nhất 1 thuốc")]
        public List<CreateOrderDetailDto> Details { get; set; } = new();

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [MaxLength(30)]
        public string PaymentMethod { get; set; } = "Tiền mặt";
    }
}