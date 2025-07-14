using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.DTOs.CustomerModule;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Service.Services
{
    public class CustomerServiceImpl : ICustomerService
    {
        private readonly LoanApplicationServiceDbContext _db;
        private readonly IMapper _mapper;

        public CustomerServiceImpl(LoanApplicationServiceDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var customers = await _db.Customers
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return _mapper.Map<List<CustomerDto>>(customers);
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id && !c.IsDeleted);
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> CreateAsync(Customer customer)
        {
            _db.Customers.Add(customer);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(CustomerDto dto)
        {
            var customer = await _db.Customers.FindAsync(dto.CustomerId);
            if (customer == null) return false;

            _mapper.Map(dto, customer);
            _db.Customers.Update(customer);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return false;

            customer.IsDeleted = true;
            _db.Customers.Update(customer);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
