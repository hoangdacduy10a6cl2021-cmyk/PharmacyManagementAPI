using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    public class SupplierFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
        [Display(Name = "Tên nhà cung cấp")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}