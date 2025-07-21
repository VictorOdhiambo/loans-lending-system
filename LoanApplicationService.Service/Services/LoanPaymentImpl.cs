using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.DTOs.LoanPayment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Service.Services
{
    public class LoanPaymentImpl (LoanApplicationServiceDbContext context, IMapper mapper, ILoanApplicationService loanApplicationService) : ILoanPaymentService
    {
        private readonly LoanApplicationServiceDbContext _context = context;
        private readonly IMapper _mapper = mapper;          
        private readonly ILoanApplicationService _loanApplicationService = loanApplicationService;
        public async Task<bool> MakePaymentAsync(int accountId, LoanPaymentDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var account = await _context.Accounts.FindAsync(accountId);
                if (account == null) return false;

                var payment = new LoanPayment
                {
                    AccountId = dto.AccountId,
                    Amount = dto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = dto.PaymentMethod
                };

                account.OutstandingBalance -= dto.Amount;
                account.UpdatedAt = DateTime.UtcNow;
                _context.Accounts.Update(account);
                await _context.LoanPayment.AddAsync(payment);

                await _context.SaveChangesAsync();

                if (account.OutstandingBalance < 0)
                {
                    var application = await _context.LoanApplications.FindAsync(account.ApplicationId);
                    if (application != null)
                    {
                        application.Status = (int)LoanStatus.Closed;
                        application.UpdatedAt = DateTime.UtcNow;
                        _context.LoanApplications.Update(application);
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<IEnumerable<LoanPaymentDto>> GetPaymentsByAccountIdAsync(int accountId)
        {
            var payments = await _context.LoanPayment
                .Where(p => p.AccountId == accountId)
                .ToListAsync();
            return payments.Select(p => new LoanPaymentDto
            {
                Id = p.Id,
                AccountId = p.AccountId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod
            });
        }
        public async Task<LoanPaymentDto> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _context.LoanPayment.FindAsync(paymentId);
            if (payment == null) return null;
            return new LoanPaymentDto
            {
                Id = payment.Id,
                AccountId = payment.AccountId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod
            };
        }
        public async Task<IEnumerable<LoanPaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _context.LoanPayment.ToListAsync();
            return payments.Select(p => new LoanPaymentDto
            {
                Id = p.Id,
                AccountId = p.AccountId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod
            });
        }
    }
   
}









