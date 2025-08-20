using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.Service.DTOs.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static LoanApplicationService.Service.Services.LoanPaymentImpl;
using Microsoft.AspNetCore.Http;


namespace LoanApplicationService.Service.Services
{
    public class LoanPaymentImpl(LoanApplicationServiceDbContext context, IMapper mapper, ILoanApplicationService loanApplicationService, IRepaymentScheduleService repaymentScheduleService, ILogger<LoanPaymentImpl> logger
        ) : ILoanPaymentService
    {
        private readonly LoanApplicationServiceDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly ILoanApplicationService _loanApplicationService = loanApplicationService;
        private readonly IRepaymentScheduleService _repaymentScheduleService = repaymentScheduleService;
        private readonly ILogger<LoanPaymentImpl> _logger = logger;


        public record PaymentResult(
            bool Success,
            decimal AmountReceived,
            decimal PrincipalPaid,
            decimal InterestPaid,
            decimal NewOutstandingBalance,
            DateTime? NextPaymentDate,
            bool LoanClosed
        );


        public async Task<PaymentResult> MakePaymentAsync(LoanPaymentDto loanPaymentDto, Guid userId, string userIpAddress, CancellationToken ct = default)
        {
            const decimal FinancialThreshold = 0.01m;

            if (loanPaymentDto == null || loanPaymentDto.Amount <= 0)
            {
                _logger.LogWarning("Invalid payment amount or DTO received for accountId: {AccountId}", loanPaymentDto?.AccountId);
                throw new ArgumentException("Invalid payment amount or DTO.", nameof(loanPaymentDto));
            }

            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var account = await _context.Accounts
                    .AsTracking()
                    .FirstOrDefaultAsync(a => a.AccountId == loanPaymentDto.AccountId, ct);

                if (account == null)
                {
                    _logger.LogError("Account with ID {AccountId} not found for payment.", loanPaymentDto.AccountId);
                    return new PaymentResult(false, 0, 0, 0, 0, null, false);
                }

                decimal paymentRemaining = Math.Round(loanPaymentDto.Amount, 2, MidpointRounding.AwayFromZero);
                decimal totalPenaltiesPaid = 0m;
                decimal totalInterestPaid = 0m;
                decimal totalPrincipalPaid = 0m;

                // Step 1: Pay unpaid penalties
                var unpaidPenalties = await _context.LoanPenalties
                    .Where(p => p.AccountId == account.AccountId && !p.IsPaid)
                    .OrderBy(p => p.AppliedDate)
                    .ToListAsync(ct);

                foreach (var penalty in unpaidPenalties)
                {
                    if (paymentRemaining <= FinancialThreshold) break;

                    var amountToPayPenalty = Math.Min(paymentRemaining, penalty.Amount);
                    penalty.Amount -= amountToPayPenalty;
                    paymentRemaining -= amountToPayPenalty;
                    totalPenaltiesPaid += amountToPayPenalty;

                    if (penalty.Amount <= FinancialThreshold)
                    {
                        penalty.IsPaid = true;
                        penalty.Amount = 0;
                    }
                    _context.LoanPenalties.Update(penalty);
                }


                // Step 2: Pay due and unpaid installments
                var dueAndUnpaidScheduleItems = await _context.LoanRepaymentSchedules
                    .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid && s.StartDate <= DateTime.UtcNow)
                    .OrderBy(s => s.DueDate)
                    .ThenBy(s => s.InstallmentNumber)
                    .ToListAsync(ct);

                foreach (var item in dueAndUnpaidScheduleItems)
                {
                    if (paymentRemaining <= FinancialThreshold) break;

                    var interestDue = item.InterestAmount - item.PaidInterest;
                    if (interestDue > FinancialThreshold)
                    {
                        var interestPayment = Math.Min(paymentRemaining, interestDue);
                        item.PaidInterest += interestPayment;
                        paymentRemaining -= interestPayment;
                        totalInterestPaid += interestPayment;
                    }

                    if (paymentRemaining > FinancialThreshold)
                    {
                        var principalDue = item.PrincipalAmount - item.PaidPrincipal;
                        if (principalDue > FinancialThreshold)
                        {
                            var principalPayment = Math.Min(paymentRemaining, principalDue);
                            item.PaidPrincipal += principalPayment;
                            paymentRemaining -= principalPayment;
                            totalPrincipalPaid += principalPayment;
                            account.OutstandingBalance -= principalPayment;

                        }
                    }

                    if (item.PaidInterest >= item.InterestAmount - FinancialThreshold &&
                        item.PaidPrincipal >= item.PrincipalAmount - FinancialThreshold)
                    {
                        item.IsPaid = true;
                        item.PaidDate = DateTime.UtcNow;
                    }

                    _context.LoanRepaymentSchedules.Update(item);
                }

                // Step 3: Apply any remaining payment to future installments' principal
                if (paymentRemaining > FinancialThreshold)
                {
                    var futureUnpaidScheduleItems = await _context.LoanRepaymentSchedules
                        .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid && s.StartDate > DateTime.UtcNow)
                        .OrderBy(s => s.DueDate)
                        .ToListAsync(ct);

                    foreach (var item in futureUnpaidScheduleItems)
                    {
                        if (paymentRemaining <= FinancialThreshold) break;

                        var principalDue = item.PrincipalAmount - item.PaidPrincipal;
                        if (principalDue > FinancialThreshold)
                        {
                            var principalPayment = Math.Min(paymentRemaining, principalDue);
                            item.PaidPrincipal += principalPayment;
                            paymentRemaining -= principalPayment;
                            totalPrincipalPaid += principalPayment;

                            account.OutstandingBalance -= principalPayment;
                        }

                        if (item.PaidPrincipal >= item.PrincipalAmount - FinancialThreshold &&
                            item.PaidInterest >= item.InterestAmount - FinancialThreshold)
                        {
                            item.IsPaid = true;
                            item.PaidDate = DateTime.UtcNow;
                        }

                        _context.LoanRepaymentSchedules.Update(item);
                    }
                }


                account.OutstandingBalance = Math.Max(0, account.OutstandingBalance);

                if (account.OutstandingBalance <= FinancialThreshold)
                {
                    var remainingUnpaid = await _context.LoanRepaymentSchedules
                        .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid)
                        .ToListAsync(ct);
                    if (remainingUnpaid.Any())
                    {
                        _context.LoanRepaymentSchedules.RemoveRange(remainingUnpaid);
                    }
                }

                // Step 5: Update account and create transaction
                account.UpdatedAt = DateTimeOffset.Now;

                var nextUnpaidSchedule = await _context.LoanRepaymentSchedules
                    .AsNoTracking()
                    .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid)
                    .OrderBy(s => s.DueDate)
                    .FirstOrDefaultAsync(ct);

                account.NextPaymentDate = nextUnpaidSchedule?.DueDate;
                _context.Accounts.Update(account);

                var transaction = new Transactions
                {
                    AccountId = loanPaymentDto.AccountId,
                    Amount = loanPaymentDto.Amount,
                    PrincipalAmount = Math.Round(totalPrincipalPaid, 2),
                    InterestAmount = Math.Round(totalInterestPaid, 2),
                    PenaltyAmount = Math.Round(totalPenaltiesPaid, 2),
                    TransactionDate = DateTimeOffset.Now,
                    TransactionType = (int)TransactionType.Payment,
                    PaymentMethod = loanPaymentDto.PaymentMethod,
                };
                await _context.Transactions.AddAsync(transaction, ct);

                //record payment log
                var paymentLog = await RecordPaymentLog(loanPaymentDto.AccountId);
                paymentLog.UserId = userId;
                paymentLog.IpAddress = userIpAddress;
                paymentLog.OldValues = (account.OutstandingBalance + totalPrincipalPaid) .ToString();
                paymentLog.NewValues = account.OutstandingBalance.ToString();

                _context.AuditTrail.Add(paymentLog);

                bool loanClosed = false;
                if (account.OutstandingBalance <= FinancialThreshold)
                {
                    loanClosed = await CloseLoanIfApplicable(account, FinancialThreshold, ct);
                }

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return new PaymentResult(
                    true,
                    loanPaymentDto.Amount,
                    Math.Round(totalPrincipalPaid, 2),
                    Math.Round(totalInterestPaid, 2),
                    Math.Round(account.OutstandingBalance, 2),
                    account.NextPaymentDate,
                    loanClosed
                );
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "Payment processing failed for account {AccountId}: {Message}", loanPaymentDto.AccountId, ex.Message);
                throw new InvalidOperationException($"Failed to process payment for account {loanPaymentDto.AccountId}.", ex);
            }
        }
        private DateTime GetNextDueDate(DateTime currentDueDate, PaymentFrequency frequency)
        {
            return frequency switch
            {
                PaymentFrequency.Monthly => currentDueDate.AddMonths(1),
                PaymentFrequency.Biweekly => currentDueDate.AddDays(14),
                PaymentFrequency.Weekly => currentDueDate.AddDays(7),
                PaymentFrequency.Quarterly => currentDueDate.AddMonths(3),
                _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Unsupported payment frequency."),
            };
        }

        public async Task<IEnumerable<LoanPaymentDto>> GetPaymentsByAccountIdAsync(int accountId)
        {
            var payments = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.TransactionType == (int)TransactionType.Payment)
                .Select(t => new LoanPaymentDto
                {
                    Id = t.TransactionId,
                    AccountId = t.AccountId,
                    Amount = t.Amount,
                    PaymentDate = t.TransactionDate,
                    PaymentMethod = t.PaymentMethod
                })
                .ToListAsync();
            return payments;
        }

        public async Task<IEnumerable<LoanPaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _context.Transactions
                .Where(t => t.TransactionType == (int)TransactionType.Payment)
                .ToListAsync();
            return payments.Select(t => new LoanPaymentDto
            {
                Id = t.TransactionId,
                AccountId = t.AccountId,
                Amount = t.Amount,
                PaymentDate = t.TransactionDate,
                PaymentMethod = t.PaymentMethod
            }).ToList();
        }

        public async Task<decimal> GetTotalArrears(int accountId)
        {
            var arrears = await _context.LoanRepaymentSchedules
                .Where(s => s.AccountId == accountId && s.DueDate < DateTime.UtcNow && !s.IsPaid)
                .ToListAsync();

            return arrears.Sum(a => (a.PrincipalAmount - a.PaidPrincipal) +
                                     (a.InterestAmount - a.PaidInterest));
        }

        private async Task<bool> CloseLoanIfApplicable(Account account, decimal financialThreshold, CancellationToken ct)
        {
            if (account.OutstandingBalance > financialThreshold) return false;

            var hasUnpaidPenalties = await _context.LoanPenalties
                .AnyAsync(p => p.AccountId == account.AccountId && p.Amount > financialThreshold, ct);

            var hasUnpaidSchedules = await _context.LoanRepaymentSchedules
                .AnyAsync(s => s.AccountId == account.AccountId && !s.IsPaid &&
                               (s.PrincipalAmount - s.PaidPrincipal > financialThreshold ||
                                s.InterestAmount - s.PaidInterest > financialThreshold), ct);

            if (hasUnpaidPenalties || hasUnpaidSchedules) return false;

            var application = await _context.LoanApplications
                .FirstOrDefaultAsync(x => x.ApplicationId == account.ApplicationId, ct);

            if (application != null)
            {
                application.Status = (int)LoanStatus.Closed;
                application.UpdatedAt = DateTime.UtcNow;
                _context.LoanApplications.Update(application);

                account.Status = (int)AccountStatus.Closed;
                account.MaturityDate = DateTime.UtcNow;
                account.NextPaymentDate = null;

                _context.Accounts.Update(account);
            }

            _logger.LogInformation("Loan {AccountId} has been closed.", account.AccountId);
            return true;
        }

        public async Task<AuditTrail> RecordPaymentLog(int accountId)
        {

            var account = await _context.Accounts
                .Where(a => a.AccountId == accountId)
                .FirstOrDefaultAsync();

            

            var PaymentLog = new AuditTrail
            {
                CustomerId = account.CustomerId,
                ApplicationId = account.ApplicationId,
                AccountId = accountId,
                EntityType = "Account",
                EntityId = accountId,
                Action = "Payment",
                CreatedAt = DateTime.UtcNow,
               Customer = account.Customer,
                LoanApplication = account.LoanApplication,
                Account = account

            };

            return PaymentLog;
        }

        
    }
}
