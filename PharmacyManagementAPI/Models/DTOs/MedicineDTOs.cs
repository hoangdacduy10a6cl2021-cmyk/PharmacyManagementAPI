using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class MedicineDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal SellPrice { get; set; }
        public decimal ImportPrice { get; set; }
        public int Stock { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Barcode { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateMedicineDto
    {
        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public int? SupplierId { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "Hộp";

        [Range(0, double.MaxValue)]
        public decimal SellPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; } = 0;

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(50)]
        public string? Barcode { get; set; }
    }

    public class UpdateMedicineDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public int? SupplierId { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "Hộp";

        [Range(0, double.MaxValue)]
        public decimal SellPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(50)]
        public string? Barcode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}