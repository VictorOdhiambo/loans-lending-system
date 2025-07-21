using LoanApplicationService.Service.DTOs.LoanPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public interface ILoanPaymentService
    {
        Task<bool> MakePaymentAsync(int accountId, LoanPaymentDto dto);
        
        Task<IEnumerable<LoanPaymentDto>> GetPaymentsByAccountIdAsync(int accountId);
        
        Task<LoanPaymentDto> GetPaymentByIdAsync(int paymentId);
       
        Task<IEnumerable<LoanPaymentDto>> GetAllPaymentsAsync();
    }
}
