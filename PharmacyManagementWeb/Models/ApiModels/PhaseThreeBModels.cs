using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    // Dùng cho cả hiển thị và form Cài đặt cửa hàng
    public class StoreSettingModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cửa hàng")]
        [MaxLength(100)]
        [Display(Name = "Tên cửa hàng")]
        public string StoreName { get; set; } = "Tâm Thần Pharmacy";

        [MaxLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [MaxLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [MaxLength(30)]
        [Display(Name = "Mã số thuế")]
        public string? TaxCode { get; set; }

        [Range(0, 100000, ErrorMessage = "Ngưỡng phải từ 0 đến 100000")]
        [Display(Name = "Sắp hết hàng khi tồn kho từ (trở xuống)")]
        public int LowStockThreshold { get; set; } = 20;

        [Range(1, 365, ErrorMessage = "Số ngày phải từ 1 đến 365")]
        [Display(Name = "Cảnh báo sắp hết hạn trước (ngày)")]
        public int ExpiringSoonDays { get; set; } = 90;

        [Range(0, 100, ErrorMessage = "Phần trăm phải từ 0 đến 100")]
        [Display(Name = "Giảm giá tối đa của nhân viên (%)")]
        public decimal MaxStaffDiscountPercent { get; set; } = 10;

        [MaxLength(300)]
        [Display(Name = "Lời cảm ơn in cuối hoá đơn")]
        public string? ReceiptFooter { get; set; }
    }

    public class ConsultationMedicineModel
    {
        public int MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public decimal SellPrice { get; set; }
        public int Stock { get; set; }
    }

    public class ConsultationModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Advice { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ConsultationMedicineModel> Medicines { get; set; } = new();
    }
}