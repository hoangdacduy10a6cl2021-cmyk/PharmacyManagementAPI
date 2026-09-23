using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementAPI.Models.Entities
{
    public class PrescriptionDetail
    {
        [Key]
        public int Id { get; set; }

        public int PrescriptionId { get; set; }
        [ForeignKey(nameof(PrescriptionId))]
        public Prescription? Prescription { get; set; }

        public int MedicineId { get; set; }
        [ForeignKey(nameof(MedicineId))]
        public Medicine? Medicine { get; set; }

        [MaxLength(100)]
        public string? Dosage { get; set; } // "3 lần/ngày - 7 ngày"

        public int Quantity { get; set; }
    }
}