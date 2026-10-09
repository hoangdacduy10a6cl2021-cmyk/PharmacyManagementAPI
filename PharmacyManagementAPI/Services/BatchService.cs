using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IBatchService
    {
        // Thêm lô mới (chưa SaveChanges, người gọi tự lưu rồi gọi SyncMedicineAsync)
        MedicineBatch AddBatch(int medicineId, string? batchNumber, DateTime? expiry, decimal importPrice,
                               int quantity, int? purchaseOrderId, string? note);

        // Số lượng thuốc còn hạn có thể bán
        Task<int> GetSellableQuantityAsync(int medicineId);

        // Trừ kho theo FEFO, trả về danh sách (lô, số lượng lấy). Lỗi nếu không đủ hàng còn hạn
        Task<(List<(MedicineBatch batch, int qty)>? allocations, string? error)> DeductFefoAsync(int medicineId, int quantity);

        // Trả lại số lượng vào lô đã bán ra, hoặc vào lô gần nhất nếu hoá đơn cũ chưa có thông tin lô
        Task RestoreForOrderDetailAsync(int orderDetailId, int medicineId, int quantity);

        // Đồng bộ Medicine.Stock và Medicine.ExpiryDate theo các lô (gọi sau khi đã SaveChanges)
        Task SyncMedicineAsync(Medicine medicine);

        // Điều chỉnh tồn kho về đúng số đếm thực tế (dùng cho kiểm kê)
        Task AdjustToQuantityAsync(Medicine medicine, int actualQuantity, string note);

        // Tạo lô đầu kỳ cho thuốc đang có tồn kho nhưng chưa có lô
        Task EnsureInitialBatchesAsync();

        Task<List<MedicineBatchDto>> GetBatchesAsync(int medicineId);
        Task<(MedicineBatchDto? data, string? error)> DisposeAsync(int batchId);
    }

    public class BatchService : IBatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStoreSettingService _settings;
        private readonly ILogger<BatchService> _logger;

        public BatchService(ApplicationDbContext context, IStoreSettingService settings, ILogger<BatchService> logger)
        {
            _context = context;
            _settings = settings;
            _logger = logger;
        }

        public MedicineBatch AddBatch(int medicineId, string? batchNumber, DateTime? expiry, decimal importPrice,
                                      int quantity, int? purchaseOrderId, string? note)
        {
            var batch = new MedicineBatch
            {
                MedicineId = medicineId,
                BatchNumber = string.IsNullOrWhiteSpace(batchNumber)
                    ? $"LO{DateTime.Now:yyMMddHHmmss}{medicineId}"
                    : batchNumber.Trim(),
                ExpiryDate = expiry?.Date,
                ImportPrice = importPrice,
                InitialQuantity = quantity,
                Quantity = quantity,
                ReceivedDate = DateTime.Now,
                PurchaseOrderId = purchaseOrderId,
                Note = note
            };

            _context.MedicineBatches.Add(batch);
            return batch;
        }

        public async Task<int> GetSellableQuantityAsync(int medicineId)
        {
            var today = DateTime.Today;
            return await _context.MedicineBatches
                .Where(b => b.MedicineId == medicineId && b.Quantity > 0
                            && (b.ExpiryDate == null || b.ExpiryDate >= today))
                .SumAsync(b => (int?)b.Quantity) ?? 0;
        }

        public async Task<(List<(MedicineBatch batch, int qty)>? allocations, string? error)> DeductFefoAsync(int medicineId, int quantity)
        {
            var today = DateTime.Today;

            // Lô gần hết hạn bán trước, lô không có hạn bán sau cùng
            var batches = await _context.MedicineBatches
                .Where(b => b.MedicineId == medicineId && b.Quantity > 0
                            && (b.ExpiryDate == null || b.ExpiryDate >= today))
                .OrderBy(b => b.ExpiryDate == null)
                .ThenBy(b => b.ExpiryDate)
                .ThenBy(b => b.Id)
                .ToListAsync();

            if (batches.Sum(b => b.Quantity) < quantity)
                return (null, "Không đủ số lượng thuốc còn hạn.");

            var result = new List<(MedicineBatch, int)>();
            var remaining = quantity;

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;
                var take = Math.Min(batch.Quantity, remaining);
                batch.Quantity -= take;
                remaining -= take;
                result.Add((batch, take));
            }

            return (result, null);
        }

        public async Task RestoreForOrderDetailAsync(int orderDetailId, int medicineId, int quantity)
        {
            var remaining = quantity;

            var allocations = await _context.OrderDetailBatches
                .Include(a => a.Batch)
                .Where(a => a.OrderDetailId == orderDetailId && a.Quantity > a.ReturnedQuantity)
                .OrderBy(a => a.Id)
                .ToListAsync();

            foreach (var a in allocations)
            {
                if (remaining <= 0) break;
                var capacity = a.Quantity - a.ReturnedQuantity;
                var put = Math.Min(capacity, remaining);
                a.ReturnedQuantity += put;
                a.Batch!.Quantity += put;
                remaining -= put;
            }

            // Hoá đơn tạo trước khi có quản lý lô: trả vào lô gần nhất, nếu chưa có lô thì tạo lô mới
            if (remaining > 0)
            {
                var latest = await _context.MedicineBatches
                    .Where(b => b.MedicineId == medicineId)
                    .OrderByDescending(b => b.ExpiryDate)
                    .ThenByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                if (latest != null)
                {
                    latest.Quantity += remaining;
                }
                else
                {
                    var medicine = await _context.Medicines.FindAsync(medicineId);
                    AddBatch(medicineId, "LO-TRALAI", medicine?.ExpiryDate, medicine?.ImportPrice ?? 0,
                             remaining, null, "Thuốc trả lại từ hoá đơn cũ");
                }
            }
        }

        public async Task SyncMedicineAsync(Medicine medicine)
        {
            var batches = await _context.MedicineBatches
                .Where(b => b.MedicineId == medicine.Id && b.Quantity > 0)
                .ToListAsync();

            medicine.Stock = batches.Sum(b => b.Quantity);
            medicine.ExpiryDate = batches.Where(b => b.ExpiryDate.HasValue).Min(b => b.ExpiryDate);
        }

        public async Task AdjustToQuantityAsync(Medicine medicine, int actualQuantity, string note)
        {
            await _context.SaveChangesAsync();
            await SyncMedicineAsync(medicine);

            var diff = actualQuantity - medicine.Stock;

            if (diff > 0)
            {
                // Thừa: tạo lô kiểm kê, lấy hạn của lô còn hàng muộn nhất (hoặc hạn hiện tại của thuốc)
                var latestExpiry = await _context.MedicineBatches
                    .Where(b => b.MedicineId == medicine.Id && b.Quantity > 0)
                    .MaxAsync(b => b.ExpiryDate);

                AddBatch(medicine.Id, $"KK{DateTime.Now:yyMMddHHmm}", latestExpiry ?? medicine.ExpiryDate,
                         medicine.ImportPrice, diff, null, note);
            }
            else if (diff < 0)
            {
                // Thiếu: trừ dần từ lô gần hết hạn nhất (kể cả lô đã hết hạn)
                var need = -diff;
                var batches = await _context.MedicineBatches
                    .Where(b => b.MedicineId == medicine.Id && b.Quantity > 0)
                    .OrderBy(b => b.ExpiryDate == null)
                    .ThenBy(b => b.ExpiryDate)
                    .ThenBy(b => b.Id)
                    .ToListAsync();

                foreach (var b in batches)
                {
                    if (need <= 0) break;
                    var take = Math.Min(b.Quantity, need);
                    b.Quantity -= take;
                    need -= take;
                }
            }

            await _context.SaveChangesAsync();
            await SyncMedicineAsync(medicine);
        }

        // Dữ liệu cũ: thuốc có tồn kho nhưng chưa có lô nào thì tạo lô đầu kỳ
        public async Task EnsureInitialBatchesAsync()
        {
            var medicines = await _context.Medicines.Where(m => m.Stock > 0).ToListAsync();
            if (!medicines.Any()) return;

            var batched = await _context.MedicineBatches
                .GroupBy(b => b.MedicineId)
                .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.Key, x => x.Qty);

            var created = 0;
            foreach (var m in medicines)
            {
                var have = batched.TryGetValue(m.Id, out var q) ? q : 0;
                var missing = m.Stock - have;
                if (missing > 0)
                {
                    AddBatch(m.Id, "LO-DAU", m.ExpiryDate, m.ImportPrice, missing, null, "Lô đầu kỳ (tự tạo từ tồn kho cũ)");
                    created++;
                }
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã tạo {Count} lô đầu kỳ cho thuốc có tồn kho cũ", created);
            }
        }

        private static MedicineBatchDto ToDto(MedicineBatch b, DateTime today, int soonDays) => new MedicineBatchDto
        {
            Id = b.Id,
            MedicineId = b.MedicineId,
            MedicineName = b.Medicine?.Name ?? "",
            MedicineCode = b.Medicine?.Code ?? "",
            BatchNumber = b.BatchNumber,
            ExpiryDate = b.ExpiryDate,
            DaysToExpire = b.ExpiryDate.HasValue ? (int)(b.ExpiryDate.Value.Date - today).TotalDays : null,
            ImportPrice = b.ImportPrice,
            InitialQuantity = b.InitialQuantity,
            Quantity = b.Quantity,
            ReceivedDate = b.ReceivedDate,
            Note = b.Note,
            Status = b.Quantity <= 0 ? "Đã hết"
                   : b.ExpiryDate.HasValue && b.ExpiryDate.Value.Date < today ? "Hết hạn"
                   : b.ExpiryDate.HasValue && b.ExpiryDate.Value.Date <= today.AddDays(soonDays) ? "Sắp hết hạn"
                   : "Còn hạn"
        };

        public async Task<List<MedicineBatchDto>> GetBatchesAsync(int medicineId)
        {
            var cfg = await _settings.GetEntityAsync();
            var today = DateTime.Today;

            var batches = await _context.MedicineBatches
                .Include(b => b.Medicine)
                .Where(b => b.MedicineId == medicineId)
                .OrderBy(b => b.Quantity == 0)
                .ThenBy(b => b.ExpiryDate == null)
                .ThenBy(b => b.ExpiryDate)
                .ThenBy(b => b.Id)
                .ToListAsync();

            return batches.Select(b => ToDto(b, today, cfg.ExpiringSoonDays)).ToList();
        }

        // Hủy lô hết hạn: đưa số lượng về 0
        public async Task<(MedicineBatchDto? data, string? error)> DisposeAsync(int batchId)
        {
            var batch = await _context.MedicineBatches.Include(b => b.Medicine).FirstOrDefaultAsync(b => b.Id == batchId);
            if (batch == null) return (null, "Không tìm thấy lô thuốc.");
            if (batch.Quantity <= 0) return (null, "Lô này đã hết, không còn gì để hủy.");

            if (!batch.ExpiryDate.HasValue || batch.ExpiryDate.Value.Date >= DateTime.Today)
                return (null, "Chỉ được hủy lô đã hết hạn. Nếu số lượng bị lệch, hãy dùng chức năng Kiểm kê kho.");

            var destroyed = batch.Quantity;
            batch.Quantity = 0;
            batch.Note = $"Đã hủy {destroyed} do hết hạn ngày {DateTime.Now:dd/MM/yyyy}";

            await _context.SaveChangesAsync();
            await SyncMedicineAsync(batch.Medicine!);
            await _context.SaveChangesAsync();

            var cfg = await _settings.GetEntityAsync();
            return (ToDto(batch, DateTime.Today, cfg.ExpiringSoonDays), null);
        }
    }
}