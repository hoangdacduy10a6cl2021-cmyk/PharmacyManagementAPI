using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementAPI.Models.Entities
{
    public class ReturnOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required, MaxLength(100)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Note { get; set; }

        // Có nhập lại thuốc vào kho khi duyệt hay không
        public bool RestockItems { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [MaxLength(30)]
        public string RefundMethod { get; set; } = "Tiền mặt";

        // Chờ duyệt / Đã duyệt / Từ chối
        [Required, MaxLength(30)]
        public string Status { get; set; } = "Chờ duyệt";

        public int? ReviewedByUserId { get; set; }
        public User? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [MaxLength(500)]
        public string? ReviewNote { get; set; }

        public List<ReturnDetail> ReturnDetails { get; set; } = new();
    }

    public class ReturnDetail
    {
        [Key]
        public int Id { get; set; }

        public int ReturnOrderId { get; set; }
        public ReturnOrder? ReturnOrder { get; set; }

        public int OrderDetailId { get; set; }
        public OrderDetail? OrderDetail { get; set; }

        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}