using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string CurrentStatus { get; set; } = string.Empty; // "Đang diễn ra" / "Chưa bắt đầu" / "Đã kết thúc"
    }

    public class CreatePromotionDto
    {
        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // "PhanTram" hoặc "SoTien"
        [Required, MaxLength(20)]
        public string DiscountType { get; set; } = "PhanTram";

        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class UpdatePromotionDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string DiscountType { get; set; } = "PhanTram";

        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}