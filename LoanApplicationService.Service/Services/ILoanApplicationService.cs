using LoanApplicationService.Service.DTOs.LoanApplicationModule;

namespace LoanApplicationService.Service.Services
{
    public interface ILoanApplicationService
    {
        Task<bool> CreateAsync(LoanApplicationDto dto);
        Task<IEnumerable<LoanApplicationDto>> GetAllAsync();
        Task<LoanApplicationDto> GetByIdAsync(int applicationId);
        Task<bool> ApproveAsync(int applicationId, decimal approvedAmount);
        Task<bool> RejectAsync(int applicationId);

        Task<bool> CloseAsync(int applicationId, string decisionNotes);

        Task <IEnumerable<LoanApplicationDto>>GetByCustomerIdAsync(int customerId);

        Task<bool>CustomerReject(int applicationId, string reason); 
        Task<bool> DisburseAsync(int applicationId);
    }
}
