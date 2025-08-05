using LoanApplicationService.Service.DTOs.LoanDisbursement;
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
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Transactions;


namespace LoanApplicationService.Service.Services
{
    public class LoanWithdrawalServiceImpl(LoanApplicationServiceDbContext loanApplicationServiceDbContext, IMapper mapper) : ILoanWithdrawalService
    {
        private readonly LoanApplicationServiceDbContext _context = loanApplicationServiceDbContext;
        private readonly IMapper _mapper = mapper;


        public async Task<bool> WithdrawAsync(LoanWithdawalDto loanWithdawalDto)
        {
            var account = await _context.Accounts.FindAsync(loanWithdawalDto.AccountId);
            if (account == null || account.Status != (int)AccountStatus.Active)
                return false;

            if (loanWithdawalDto.Amount <= 0 || loanWithdawalDto.Amount > account.AvailableBalance)
                return false;

            account.AvailableBalance -= loanWithdawalDto.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            var withdrawalTransaction = new Transactions
            {
                AccountId = loanWithdawalDto.AccountId,
                Amount = loanWithdawalDto.Amount,
                TransactionType = (int)TransactionType.Withdrawal,
                PaymentMethod = loanWithdawalDto.PaymentMethod,
                TransactionDate = DateTimeOffset.UtcNow
            };


            _context.Accounts.Update(account);
            await _context.Transactions.AddAsync(withdrawalTransaction);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(int accountId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }
    }
}
