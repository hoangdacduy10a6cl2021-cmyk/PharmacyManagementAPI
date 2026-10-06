using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.Entities
{
    public class StockCheck
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public List<StockCheckDetail> Details { get; set; } = new();
    }

    public class StockCheckDetail
    {
        [Key]
        public int Id { get; set; }

        public int StockCheckId { get; set; }
        public StockCheck? StockCheck { get; set; }

        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }

        // Tồn trên hệ thống tại thời điểm kiểm kê
        public int SystemQty { get; set; }

        // Số đếm thực tế
        public int ActualQty { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }
    }
}