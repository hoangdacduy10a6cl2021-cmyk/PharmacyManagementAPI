using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IStockCheckService
    {
        Task<List<StockCheckDto>> GetAllAsync();
        Task<StockCheckDto?> GetByIdAsync(int id);
        Task<(StockCheckDto? data, string? error)> CreateAsync(CreateStockCheckDto dto, int userId);
    }

    public class StockCheckService : IStockCheckService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBatchService _batches;

        public StockCheckService(ApplicationDbContext context, IBatchService batches)
        {
            _context = context;
            _batches = batches;
        }

        private IQueryable<StockCheck> Query() => _context.StockChecks
            .Include(s => s.User)
            .Include(s => s.Details).ThenInclude(d => d.Medicine);

        private static StockCheckDto ToDto(StockCheck s)
        {
            var details = s.Details.Select(d => new StockCheckDetailDto
            {
                MedicineId = d.MedicineId,
                MedicineName = d.Medicine?.Name ?? "",
                MedicineCode = d.Medicine?.Code ?? "",
                Unit = d.Medicine?.Unit,
                ImageUrl = d.Medicine?.ImageUrl,
                SystemQty = d.SystemQty,
                ActualQty = d.ActualQty,
                Difference = d.ActualQty - d.SystemQty,
                Note = d.Note
            }).ToList();

            return new StockCheckDto
            {
                Id = s.Id,
                Code = s.Code,
                CreatedAt = s.CreatedAt,
                UserName = s.User?.FullName,
                Note = s.Note,
                ItemCount = details.Count,
                DifferenceCount = details.Count(d => d.Difference != 0),
                SurplusQty = details.Where(d => d.Difference > 0).Sum(d => d.Difference),
                ShortageQty = details.Where(d => d.Difference < 0).Sum(d => -d.Difference),
                ValueDifference = s.Details.Sum(d => (d.ActualQty - d.SystemQty) * (d.Medicine?.ImportPrice ?? 0)),
                Details = details
            };
        }

        public async Task<List<StockCheckDto>> GetAllAsync()
        {
            var list = await Query().OrderByDescending(s => s.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<StockCheckDto?> GetByIdAsync(int id)
        {
            var s = await Query().FirstOrDefaultAsync(x => x.Id == id);
            return s == null ? null : ToDto(s);
        }

        public async Task<(StockCheckDto? data, string? error)> CreateAsync(CreateStockCheckDto dto, int userId)
        {
            var items = dto.Items.GroupBy(i => i.MedicineId).Select(g => g.Last()).ToList();
            if (!items.Any()) return (null, "Phải nhập số đếm của ít nhất 1 thuốc.");

            var ids = items.Select(i => i.MedicineId).ToList();
            var medicines = await _context.Medicines.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (medicines.Count != ids.Count) return (null, "Có thuốc trong phiếu kiểm kê không tồn tại.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var count = await _context.StockChecks.CountAsync();

                var check = new StockCheck
                {
                    Code = $"KK{(count + 1):D4}",
                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    Note = dto.Note
                };

                _context.StockChecks.Add(check);

                foreach (var item in items)
                {
                    var medicine = medicines.First(m => m.Id == item.MedicineId);

                    check.Details.Add(new StockCheckDetail
                    {
                        MedicineId = medicine.Id,
                        SystemQty = medicine.Stock,
                        ActualQty = item.ActualQty,
                        Note = item.Note
                    });

                    // Điều chỉnh các lô cho khớp số đếm thực tế (thiếu thì trừ lô gần hết hạn, thừa thì tạo lô kiểm kê)
                    await _batches.AdjustToQuantityAsync(medicine, item.ActualQty, $"Điều chỉnh từ phiếu {check.Code}");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetByIdAsync(check.Id), null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Có lỗi xảy ra khi lưu phiếu kiểm kê.");
            }
        }
    }
}