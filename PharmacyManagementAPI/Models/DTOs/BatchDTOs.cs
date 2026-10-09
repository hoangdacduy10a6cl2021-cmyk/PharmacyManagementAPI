namespace PharmacyManagementAPI.Models.DTOs
{
    public class MedicineBatchDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string MedicineCode { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public int? DaysToExpire { get; set; }
        public decimal ImportPrice { get; set; }
        public int InitialQuantity { get; set; }
        public int Quantity { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}