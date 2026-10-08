using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementAPI.Models.Entities
{
    // Chỉ có 1 dòng duy nhất, tự tạo với giá trị mặc định khi cần
    public class StoreSetting
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string StoreName { get; set; } = "Tâm Thần Pharmacy";

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? TaxCode { get; set; }

        // Thuốc có tồn kho từ mức này trở xuống bị coi là sắp hết hàng
        public int LowStockThreshold { get; set; } = 20;

        // Thuốc còn hạn dưới số ngày này bị coi là sắp hết hạn
        public int ExpiringSoonDays { get; set; } = 90;

        // % giảm giá tối đa trên mỗi hoá đơn mà nhân viên được áp dụng (Admin không bị giới hạn)
        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxStaffDiscountPercent { get; set; } = 10;

        [MaxLength(300)]
        public string? ReceiptFooter { get; set; } = "Cảm ơn quý khách. Hẹn gặp lại!";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}