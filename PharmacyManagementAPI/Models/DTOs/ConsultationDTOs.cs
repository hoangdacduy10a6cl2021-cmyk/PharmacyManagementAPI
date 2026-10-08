using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class ConsultationMedicineDto
    {
        public int MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public decimal SellPrice { get; set; }
        public int Stock { get; set; }
    }

    public class ConsultationDto
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
        public List<ConsultationMedicineDto> Medicines { get; set; } = new();
    }

    public class CreateConsultationDto
    {
        public int? CustomerId { get; set; }

        [MaxLength(100)]
        public string? CustomerName { get; set; }

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [Required, MaxLength(500)]
        public string Symptoms { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Advice { get; set; } = string.Empty;

        public DateTime? FollowUpDate { get; set; }

        public List<int> MedicineIds { get; set; } = new();
    }
}