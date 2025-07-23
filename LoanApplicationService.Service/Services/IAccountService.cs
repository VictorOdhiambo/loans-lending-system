using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;

namespace LoanApplicationService.Service.Services
{
    public interface IAccountService
    {
        Task<bool> CreateAccountAsync(AccountDto accountDto);
        Task<AccountDto> GetAccountByIdAsync(int accountId);
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
        Task<bool> UpdateAccountAsync(AccountDto accountDto);
        Task<bool> DeleteAccountAsync(int accountId);

        Task<IEnumerable<AccountDto>> GetAccountsByCustomerIdAsync(int customerId);
        Task<IEnumerable<AccountDto>> GetAccountsByApplicationIdAsync(int applicationId);
        Task<IEnumerable<AccountDto>> GetAccountsByStatusAsync(string status);

        Task<IEnumerable<AccountDto>> GetAccountsByAccountTypeAsync(string accountType);
        Task<bool> ApplyPaymentAsync(int accountId, decimal amount);

        Task<bool> WithdrawAsync(int accountId, LoanWithdawalDto dto);

    }
}


