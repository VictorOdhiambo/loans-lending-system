using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public interface ILoanWithdrawalService
    {
        Task<bool> WithdrawAsync(LoanWithdawalDto loanWithdawalDto);

        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync( int accountId);



    }
}
