using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagementAPI.Models.Entities
{
    public class PurchaseOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        [ForeignKey(nameof(PurchaseOrderId))]
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int MedicineId { get; set; }
        [ForeignKey(nameof(MedicineId))]
        public Medicine? Medicine { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }
    }
}