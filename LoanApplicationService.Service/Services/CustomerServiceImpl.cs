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

        public async Task<CustomerDto?> UpdateAsync(int id, CustomerDto dto)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return null;

            Console.WriteLine($"[UpdateAsync] Before update: {customer.FirstName}, {customer.LastName}, {customer.DateOfBirth}");
            Console.WriteLine($"[UpdateAsync] DTO: {dto.FirstName}, {dto.LastName}, {dto.DateOfBirth}");

            _mapper.Map(dto, customer);
            // Recalculate risk level if relevant fields changed
            int age = 0;
            if (customer.DateOfBirth.HasValue)
            {
                var today = DateTime.UtcNow;
                age = today.Year - customer.DateOfBirth.Value.Year;
                if (customer.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            }
            decimal income = customer.AnnualIncome ?? 0;
            if (age > 0 && customer.EmploymentStatus != null && customer.AnnualIncome.HasValue)
                customer.RiskLevel = LoanApplicationService.CrossCutting.Utils.RiskScoringUtil.GetRiskLevel(age, customer.EmploymentStatus, income);
            else
                customer.RiskLevel = LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh;
            _db.Entry(customer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();
            Console.WriteLine($"[UpdateAsync] After update: {customer.FirstName}, {customer.LastName}, {customer.DateOfBirth}");
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return false;

            customer.IsDeleted = true;
            _db.Customers.Update(customer);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _db.ApplicationUsers.AnyAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<CustomerDto?> GetByEmailAsync(string email)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> EmailOrNationalIdExistsAsync(string email, string? nationalId)
        {
            return await _db.Customers.AnyAsync(c => c.Email == email || (nationalId != null && c.NationalId == nationalId));
        }

        public async Task<bool> CreateUserAndCustomerAsync(CustomerDto dto)
        {
            if (await EmailOrNationalIdExistsAsync(dto.Email, dto.NationalId))
                return false;

            var role = await _db.ApplicationRoles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (role == null) return false;
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.FirstName + " " + dto.LastName,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _db.ApplicationUsers.Add(user);
            await _db.SaveChangesAsync();

            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                NationalId = dto.NationalId,
                EmploymentStatus = dto.EmploymentStatus,
                AnnualIncome = dto.AnnualIncome,
                UserId = user.Id,
                User = user
            };
            // Set RiskLevel
            int age = 0;
            if (dto.DateOfBirth.HasValue)
            {
                var today = DateTime.UtcNow;
                age = today.Year - dto.DateOfBirth.Value.Year;
                if (dto.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            }
            bool isEmployed = !string.IsNullOrWhiteSpace(dto.EmploymentStatus) && dto.EmploymentStatus.ToLower() == "employed";
            decimal income = dto.AnnualIncome ?? 0;
            if (age > 0 && dto.EmploymentStatus != null && dto.AnnualIncome.HasValue)
                customer.RiskLevel = LoanApplicationService.CrossCutting.Utils.RiskScoringUtil.GetRiskLevel(age, dto.EmploymentStatus, income);
            else
                customer.RiskLevel = LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh;
            _db.Customers.Add(customer);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
