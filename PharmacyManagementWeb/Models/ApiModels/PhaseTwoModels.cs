using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OrderCount { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "NhanVien";
    }

    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "NhanVien";

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }

    public class AuditLogModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? Description { get; set; }
        public bool IsSuccess { get; set; }
        public string? IpAddress { get; set; }
    }
}