using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? UserId { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        // Ví dụ: Đăng nhập, Thêm, Cập nhật, Xoá, Hủy, Đổi mật khẩu
        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? EntityType { get; set; }

        [MaxLength(50)]
        public string? EntityId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsSuccess { get; set; } = true;

        [MaxLength(45)]
        public string? IpAddress { get; set; }
    }
}