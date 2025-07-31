using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Service.Services
{
    public class AccountServiceImpl(LoanApplicationServiceDbContext context, IMapper mapper) : IAccountService
    {


        private readonly LoanApplicationServiceDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        public async Task<bool> CreateAccountAsync(AccountDto accountDto)
        {

            var account = _mapper.Map<Account>(accountDto);
            await _context.Accounts.AddAsync(account);
            return await _context.SaveChangesAsync() > 0;


        }
        public async Task<AccountDto> GetAccountByIdAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            return _mapper.Map<AccountDto>(account);

        }
        public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return _mapper.Map<IEnumerable<AccountDto>>(accounts);
        }

        public async Task<bool> UpdateAccountAsync(AccountDto accountDto)
        {
            var account = await _context.Accounts.FindAsync(accountDto.AccountId);
            if (account == null) return false;
            _mapper.Map(accountDto, account);
            _context.Accounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;
            account.Status = (int)AccountStatus.Closed;
            account.UpdatedAt = DateTime.UtcNow;
            _context.Accounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsByCustomerIdAsync(int customerId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AccountDto>>(accounts);
        }

        public async Task<AccountDto> GetAccountByApplicationIdAsync(int applicationId)
        {
            var account = await _context.Accounts
                .Where(a => a.ApplicationId == applicationId)
                .FirstOrDefaultAsync();

            if (account == null)
                return null;

            return _mapper.Map<AccountDto>(account);
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsByStatusAsync(int status)
        {
            var accounts = await _context.Accounts
                .Where(a => a.Status == status)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AccountDto>>(accounts);
        }  
        
        public async Task<IEnumerable<AccountDto>> GetAccountsByAccountTypeAsync(string accountType)
        {
            var accounts = await _context.Accounts
                .Where(a => a.AccountType == accountType)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AccountDto>>(accounts);
        }  

        

        public async Task<bool> GetAccountByApplicationId(int applicationId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            return account != null;
        }




    }

}
