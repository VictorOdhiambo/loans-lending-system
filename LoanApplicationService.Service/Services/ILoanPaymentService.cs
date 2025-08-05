using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.Service.DTOs.LoanPenalty;
using LoanApplicationService.Service.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoanApplicationService.Service.Services.LoanPaymentImpl;

namespace LoanApplicationService.Service.Services
{
    public interface ILoanPaymentService
    {
        Task<PaymentResult> MakePaymentAsync(LoanPaymentDto loanPaymentDto, CancellationToken ct = default);

        Task<IEnumerable<LoanPaymentDto>> GetPaymentsByAccountIdAsync(int accountId);
        
       
        Task<IEnumerable<LoanPaymentDto>> GetAllPaymentsAsync();



    }
}
