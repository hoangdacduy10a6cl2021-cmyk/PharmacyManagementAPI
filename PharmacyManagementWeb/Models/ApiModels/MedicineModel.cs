using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MedicineCount { get; set; }
    }

    public class SupplierModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MedicineModel
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

    // ViewModel dùng cho form Thêm/Sửa thuốc, kèm danh sách dropdown Category/Supplier
    public class MedicineFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã thuốc")]
        [Display(Name = "Mã thuốc")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên thuốc")]
        [Display(Name = "Tên thuốc")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public int? SupplierId { get; set; }

        [Required]
        [Display(Name = "Đơn vị tính")]
        public string Unit { get; set; } = "Hộp";

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán không hợp lệ")]
        [Display(Name = "Giá bán")]
        public decimal SellPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập không hợp lệ")]
        [Display(Name = "Giá nhập")]
        public decimal ImportPrice { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Tồn kho ban đầu")]
        public int Stock { get; set; }

        [Display(Name = "Hạn sử dụng")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Mã vạch")]
        public string? Barcode { get; set; }

        [Display(Name = "Còn kinh doanh")]
        public bool IsActive { get; set; } = true;

        public List<CategoryModel> Categories { get; set; } = new();
        public List<SupplierModel> Suppliers { get; set; } = new();
    }
}