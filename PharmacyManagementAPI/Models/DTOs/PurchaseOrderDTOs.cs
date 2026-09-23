using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class PurchaseOrderDetailDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineCode { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal Subtotal => Quantity * ImportPrice;
    }

    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<PurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class CreatePurchaseOrderDetailDto
    {
        [Required]
        public int MedicineId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; }
    }

    public class CreatePurchaseOrderDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Phiếu nhập phải có ít nhất 1 thuốc")]
        public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
    }
}