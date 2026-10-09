using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementAPI.Models.Entities
{
    // Một lô thuốc: cùng số lô, cùng hạn dùng, cùng giá nhập
    public class MedicineBatch
    {
        [Key]
        public int Id { get; set; }

        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }

        [Required, MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }

        // Số lượng nhập ban đầu và số lượng còn lại
        public int InitialQuantity { get; set; }
        public int Quantity { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        // Phiếu nhập tạo ra lô này (lô đầu kỳ, lô kiểm kê thì để trống)
        public int? PurchaseOrderId { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }
    }

    // Ghi lại hoá đơn đã lấy thuốc từ lô nào, để hủy hoá đơn / trả hàng thì trả về đúng lô
    public class OrderDetailBatch
    {
        [Key]
        public int Id { get; set; }

        public int OrderDetailId { get; set; }
        public OrderDetail? OrderDetail { get; set; }

        public int BatchId { get; set; }
        public MedicineBatch? Batch { get; set; }

        // Số lượng lấy từ lô này
        public int Quantity { get; set; }

        // Số lượng đã được trả lại lô (do hủy hoá đơn hoặc trả hàng nhập kho)
        public int ReturnedQuantity { get; set; }
    }
}