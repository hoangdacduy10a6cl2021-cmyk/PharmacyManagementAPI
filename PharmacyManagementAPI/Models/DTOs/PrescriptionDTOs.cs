using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class PrescriptionDetailDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineCode { get; set; }
        public string? Dosage { get; set; }
        public int Quantity { get; set; }
    }

    public class PrescriptionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? DoctorName { get; set; }
        public string? Diagnosis { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<PrescriptionDetailDto> Details { get; set; } = new();
    }

    public class CreatePrescriptionDetailDto
    {
        [Required]
        public int MedicineId { get; set; }

        [MaxLength(100)]
        public string? Dosage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }

    public class CreatePrescriptionDto
    {
        [Required, MaxLength(100)]
        public string PatientName { get; set; } = string.Empty;

        public int? Age { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? DoctorName { get; set; }

        [MaxLength(255)]
        public string? Diagnosis { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [Required, MinLength(1, ErrorMessage = "Đơn thuốc phải có ít nhất 1 thuốc")]
        public List<CreatePrescriptionDetailDto> Details { get; set; } = new();
    }

    public class UpdatePrescriptionStatusDto
    {
        // "Đang sử dụng" hoặc "Đã giao"
        [Required, MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}