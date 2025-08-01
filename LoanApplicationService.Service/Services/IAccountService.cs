﻿using System;
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
        Task<AccountDto> GetAccountByApplicationIdAsync(int applicationId);

        Task<IEnumerable<AccountDto>> GetAccountsByAccountTypeAsync(string accountType);



    }
}


