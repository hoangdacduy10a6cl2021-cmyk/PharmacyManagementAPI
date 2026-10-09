using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Models.Entities;
using System.Reflection.Emit;

namespace PharmacyManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<ReturnOrder> ReturnOrders { get; set; }
        public DbSet<ReturnDetail> ReturnDetails { get; set; }
        public DbSet<StockCheck> StockChecks { get; set; }
        public DbSet<StockCheckDetail> StockCheckDetails { get; set; }

        public DbSet<StoreSetting> StoreSettings { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<ConsultationMedicine> ConsultationMedicines { get; set; }
        public DbSet<MedicineBatch> MedicineBatches { get; set; }
        public DbSet<OrderDetailBatch> OrderDetailBatches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ===== Trả hàng và kiểm kê kho (tránh lỗi multiple cascade paths của SQL Server) =====
            modelBuilder.Entity<ReturnOrder>().HasIndex(r => r.Code).IsUnique();
            modelBuilder.Entity<ReturnOrder>().HasOne(r => r.Order).WithMany().HasForeignKey(r => r.OrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReturnOrder>().HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReturnOrder>().HasOne(r => r.CreatedBy).WithMany().HasForeignKey(r => r.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReturnOrder>().HasOne(r => r.ReviewedBy).WithMany().HasForeignKey(r => r.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReturnDetail>().HasOne(d => d.Medicine).WithMany().HasForeignKey(d => d.MedicineId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReturnDetail>().HasOne(d => d.OrderDetail).WithMany().HasForeignKey(d => d.OrderDetailId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockCheck>().HasIndex(s => s.Code).IsUnique();
            modelBuilder.Entity<StockCheck>().HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockCheckDetail>().HasOne(d => d.Medicine).WithMany().HasForeignKey(d => d.MedicineId).OnDelete(DeleteBehavior.Restrict);

            // ===== Tư vấn thuốc =====
            modelBuilder.Entity<Consultation>().HasIndex(c => c.Code).IsUnique();
            modelBuilder.Entity<Consultation>().HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Consultation>().HasOne(c => c.CreatedBy).WithMany().HasForeignKey(c => c.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultationMedicine>().HasOne(m => m.Medicine).WithMany().HasForeignKey(m => m.MedicineId).OnDelete(DeleteBehavior.Restrict);

            // ===== Quản lý theo lô =====
            modelBuilder.Entity<MedicineBatch>().HasOne(b => b.Medicine).WithMany().HasForeignKey(b => b.MedicineId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MedicineBatch>().HasIndex(b => new { b.MedicineId, b.ExpiryDate });
            modelBuilder.Entity<OrderDetailBatch>().HasOne(x => x.OrderDetail).WithMany().HasForeignKey(x => x.OrderDetailId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetailBatch>().HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Medicine>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<Order>().HasIndex(o => o.Code).IsUnique();
            modelBuilder.Entity<Prescription>().HasIndex(p => p.Code).IsUnique();
            modelBuilder.Entity<PurchaseOrder>().HasIndex(p => p.Code).IsUnique();
            modelBuilder.Entity<Promotion>().HasIndex(p => p.Code).IsUnique();

            // Tránh lỗi multiple cascade paths của SQL Server:
            // Medicine bị nhiều bảng chi tiết tham chiếu -> set Restrict
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Medicine)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasOne(pod => pod.Medicine)
                .WithMany(m => m.PurchaseOrderDetails)
                .HasForeignKey(pod => pod.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescriptionDetail>()
                .HasOne(pd => pd.Medicine)
                .WithMany(m => m.PrescriptionDetails)
                .HasForeignKey(pd => pd.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(p => p.User)
                .WithMany(u => u.PurchaseOrders)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.Supplier)
                .WithMany(s => s.Medicines)
                .HasForeignKey(m => m.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Medicines)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}