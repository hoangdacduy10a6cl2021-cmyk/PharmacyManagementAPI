using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class StoreSettingDto
    {
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? TaxCode { get; set; }
        public int LowStockThreshold { get; set; }
        public int ExpiringSoonDays { get; set; }
        public decimal MaxStaffDiscountPercent { get; set; }
        public string? ReceiptFooter { get; set; }
    }

    public class UpdateStoreSettingDto
    {
        [Required, MaxLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? TaxCode { get; set; }

        [Range(0, 100000, ErrorMessage = "Ngưỡng sắp hết hàng phải từ 0 đến 100000")]
        public int LowStockThreshold { get; set; } = 20;

        [Range(1, 365, ErrorMessage = "Số ngày cảnh báo hạn dùng phải từ 1 đến 365")]
        public int ExpiringSoonDays { get; set; } = 90;

        [Range(0, 100, ErrorMessage = "% giảm giá tối đa phải từ 0 đến 100")]
        public decimal MaxStaffDiscountPercent { get; set; } = 10;

        [MaxLength(300)]
        public string? ReceiptFooter { get; set; }
    }
}