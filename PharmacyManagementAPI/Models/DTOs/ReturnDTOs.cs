using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class ReturnableItemDto
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

    public class ReturnableOrderDto
    {
        public int OrderId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string? CustomerName { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<ReturnableItemDto> Items { get; set; } = new();
    }

    public class ReturnDetailDto
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

    public class ReturnOrderDto
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
        public List<ReturnDetailDto> Details { get; set; } = new();
    }

    public class CreateReturnItemDto
    {
        [Required]
        public int OrderDetailId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng trả phải lớn hơn 0")]
        public int Quantity { get; set; }
    }

    public class CreateReturnDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 thuốc để trả")]
        public List<CreateReturnItemDto> Items { get; set; } = new();

        [Required, MaxLength(100)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Note { get; set; }

        public bool RestockItems { get; set; }

        [MaxLength(30)]
        public string RefundMethod { get; set; } = "Tiền mặt";
    }

    public class ReviewReturnDto
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}