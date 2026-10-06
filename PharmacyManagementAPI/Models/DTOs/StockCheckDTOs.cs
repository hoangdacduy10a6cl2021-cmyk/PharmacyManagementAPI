using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementAPI.Models.DTOs
{
    public class StockCheckDetailDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public int SystemQty { get; set; }
        public int ActualQty { get; set; }
        public int Difference { get; set; }
        public string? Note { get; set; }
    }

    public class StockCheckDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
        public string? Note { get; set; }
        public int ItemCount { get; set; }
        public int DifferenceCount { get; set; }
        public int SurplusQty { get; set; }
        public int ShortageQty { get; set; }
        // Giá trị chênh lệch tính theo giá nhập hiện tại (âm = thiếu hụt)
        public decimal ValueDifference { get; set; }
        public List<StockCheckDetailDto> Details { get; set; } = new();
    }

    public class CreateStockCheckItemDto
    {
        [Required]
        public int MedicineId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng thực tế không được âm")]
        public int ActualQty { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }
    }

    public class CreateStockCheckDto
    {
        [Required, MinLength(1, ErrorMessage = "Phải nhập số đếm của ít nhất 1 thuốc")]
        public List<CreateStockCheckItemDto> Items { get; set; } = new();

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}