using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.CustomerModule;

namespace LoanApplicationService.Service.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Customer customer);
        Task<bool> UpdateAsync(CustomerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> UserExistsAsync(string email);
        Task<bool> CreateUserAndCustomerAsync(CustomerDto dto);
    }
}
