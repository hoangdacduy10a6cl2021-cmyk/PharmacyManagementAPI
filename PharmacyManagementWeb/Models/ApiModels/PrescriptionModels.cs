using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementWeb.Models.ApiModels
{
    public class PrescriptionDetailModel
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineCode { get; set; }
        public string? Dosage { get; set; }
        public int Quantity { get; set; }
    }

    public class PrescriptionModel
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
        public List<PrescriptionDetailModel> Details { get; set; } = new();
    }

    // Dùng để deserialize danh sách thuốc (JSON) do JavaScript gửi lên khi tạo đơn thuốc
    public class PrescriptionDetailInput
    {
        public int MedicineId { get; set; }
        public string? Dosage { get; set; }
        public int Quantity { get; set; }
    }

    public class PrescriptionFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bệnh nhân")]
        [Display(Name = "Tên bệnh nhân")]
        public string PatientName { get; set; } = string.Empty;

        [Display(Name = "Tuổi")]
        public int? Age { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Bác sĩ kê đơn")]
        public string? DoctorName { get; set; }

        [Display(Name = "Chẩn đoán")]
        public string? Diagnosis { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}