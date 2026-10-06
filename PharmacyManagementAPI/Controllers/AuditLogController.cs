using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;

namespace PharmacyManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/AuditLog?fromDate=...&toDate=...&search=...&success=false  (tối đa 500 dòng mới nhất)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? search,
            [FromQuery] bool? success)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (fromDate.HasValue) query = query.Where(a => a.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(a => a.CreatedAt <= toDate.Value);
            if (success.HasValue) query = query.Where(a => a.IsSuccess == success.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(a => a.Username.ToLower().Contains(s)
                    || a.Action.ToLower().Contains(s)
                    || (a.Description != null && a.Description.ToLower().Contains(s)));
            }

            var list = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(500)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    CreatedAt = a.CreatedAt,
                    UserId = a.UserId,
                    Username = a.Username,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Description = a.Description,
                    IsSuccess = a.IsSuccess,
                    IpAddress = a.IpAddress
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}