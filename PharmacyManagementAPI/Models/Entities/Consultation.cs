using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.Entities
{
    public class Consultation
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        // Khách hàng đã lưu trong hệ thống (không bắt buộc)
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Lưu sẵn tên và SĐT để hiển thị cả với khách vãng lai
        [Required, MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        // Triệu chứng hoặc yêu cầu của khách
        [Required, MaxLength(500)]
        public string Symptoms { get; set; } = string.Empty;

        // Nội dung dược sĩ tư vấn
        [Required, MaxLength(1000)]
        public string Advice { get; set; } = string.Empty;

        public DateTime? FollowUpDate { get; set; }

        // Cần theo dõi / Hoàn tất
        [Required, MaxLength(30)]
        public string Status { get; set; } = "Hoàn tất";

        public int CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ConsultationMedicine> Medicines { get; set; } = new();
    }

    public class ConsultationMedicine
    {
        [Key]
        public int Id { get; set; }

        public int ConsultationId { get; set; }
        public Consultation? Consultation { get; set; }

        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }
    }
}