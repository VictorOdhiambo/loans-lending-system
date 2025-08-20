using LoanApplicationService.Core.Models;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public interface IAuditService
    {
        Task AddLoanApplicationAuditAsync(int applicationId, string action, string oldStatus, string newStatus, string? remarks = null);
        Task<IEnumerable<AuditTrail>> GetAuditTrailByApplicationIdAsync(int applicationId);
        Task<IEnumerable<AuditTrail>> GetAllAuditTrailsAsync();
        Task AddLoanDisbursementAuditAsync(int applicationId, int accountId, string action, string oldStatus, string newStatus);
        Task<IEnumerable<string>> GetDistinctEntityTypesAsync();
        Task<IEnumerable<AuditTrail>> GetAuditTrailsByEntityTypeAsync(string entityType);
    }
}
