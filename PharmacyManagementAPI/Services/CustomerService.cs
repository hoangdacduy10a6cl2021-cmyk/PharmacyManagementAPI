using Microsoft.EntityFrameworkCore;
using PharmacyManagementAPI.Data;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Models.Entities;

namespace PharmacyManagementAPI.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync(string? search);
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
        Task<(bool success, string? error)> DeleteAsync(int id);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static CustomerDto ToDto(Customer c) => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Address = c.Address,
            CustomerType = c.CustomerType,
            TotalSpent = c.TotalSpent,
            CreatedAt = c.CreatedAt
        };

        public async Task<List<CustomerDto>> GetAllAsync(string? search)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search)
                    || (c.Phone != null && c.Phone.Contains(search)));
            }

            var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var c = await _context.Customers.FindAsync(id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                CustomerType = "Thường",
                TotalSpent = 0
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return ToDto(customer);
        }

        public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return null;

            customer.Name = dto.Name;
            customer.Phone = dto.Phone;
            customer.Address = dto.Address;
            customer.CustomerType = dto.CustomerType;

            await _context.SaveChangesAsync();
            return ToDto(customer);
        }

        public async Task<(bool success, string? error)> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return (false, "Không tìm thấy khách hàng.");

            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
            if (hasOrders) return (false, "Không thể xoá vì khách hàng đã có lịch sử mua hàng.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}