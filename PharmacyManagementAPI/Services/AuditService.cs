using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface IAuditService
    {
        Task LogAsync(int? userId, string username, string action, string? entityType,
                      string? entityId, string? description, bool isSuccess, string? ipAddress);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(int? userId, string username, string action, string? entityType,
                                   string? entityId, string? description, bool isSuccess, string? ipAddress)
        {
            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    Username = string.IsNullOrWhiteSpace(username) ? "(không rõ)" : username,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = description?.Length > 500 ? description[..500] : description,
                    IsSuccess = isSuccess,
                    IpAddress = ipAddress
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Ghi nhật ký lỗi không được làm hỏng thao tác chính của người dùng
                _logger.LogWarning(ex, "Không ghi được nhật ký hệ thống");
            }
        }
    }
}