using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementAPI.Models.Entities
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty; // KM001

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // "PhanTram" hoặc "SoTien"
        [MaxLength(20)]
        public string DiscountType { get; set; } = "PhanTram";

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}