using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.Entities
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty; // RX0001

        [Required, MaxLength(100)]
        public string PatientName { get; set; } = string.Empty;

        public int? Age { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? DoctorName { get; set; }

        [MaxLength(255)]
        public string? Diagnosis { get; set; }

        public DateTime PrescriptionDate { get; set; } = DateTime.Now;

        // Đang sử dụng / Đã giao
        [MaxLength(20)]
        public string Status { get; set; } = "Đang sử dụng";

        [MaxLength(500)]
        public string? Note { get; set; }

        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
    }
}