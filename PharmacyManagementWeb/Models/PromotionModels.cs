using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    public class PromotionModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
    }

    public class PromotionFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi")]
        [Display(Name = "Mã khuyến mãi")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi")]
        [Display(Name = "Tên chương trình")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; } = "PhanTram";

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị không hợp lệ")]
        [Display(Name = "Giá trị giảm")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Đang áp dụng")]
        public bool IsActive { get; set; } = true;
    }
}