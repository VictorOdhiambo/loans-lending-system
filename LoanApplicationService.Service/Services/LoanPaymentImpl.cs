using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.Service.DTOs.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoanApplicationService.Service.Services.LoanPaymentImpl;
using LoanApplicationService.Service; 

namespace LoanApplicationService.Service.Services
{
    public class LoanPaymentImpl (LoanApplicationServiceDbContext context, IMapper mapper, ILoanApplicationService loanApplicationService,IRepaymentScheduleService repaymentScheduleService, ILogger<LoanPaymentImpl> logger
        ) : ILoanPaymentService
    {
        private readonly LoanApplicationServiceDbContext _context = context;
        private readonly IMapper _mapper = mapper;          
        private readonly ILoanApplicationService _loanApplicationService = loanApplicationService;
        private readonly IRepaymentScheduleService _repaymentScheduleService = repaymentScheduleService;
        private readonly ILogger<LoanPaymentImpl> _logger = logger;


        // result type
        public record PaymentResult(
            bool Success,
            decimal AmountReceived,
            decimal PrincipalPaid,
            decimal InterestPaid,
            decimal NewOutstandingBalance,
            DateTime? NextPaymentDate,
            bool LoanClosed
        );


        public async Task<PaymentResult> MakePaymentAsync(LoanPaymentDto loanPaymentDto, CancellationToken ct = default)
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

                if (account.DisbursementDate == null)
                {
                    _logger.LogError("Disbursement date is required for account {AccountId}.", loanPaymentDto.AccountId);
                    return new PaymentResult(false, 0, 0, 0, 0, null, false);
                }

                var today = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(3)).Date; // EAT (UTC+3)

                decimal paymentRemaining = Math.Round(loanPaymentDto.Amount, 2, MidpointRounding.AwayFromZero);
                decimal totalPenaltiesPaid = 0m;
                decimal totalInterestPaid = 0m;
                decimal totalPrincipalPaid = 0m;

                // 1. Pay off penalties (if applicable)
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

                // 2. Pay off due/past-due schedule items (interest first, then principal)
                var dueAndUnpaidScheduleItems = await _context.LoanRepaymentSchedules
                    .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid && s.DueDate.Date <= today)
                    .OrderBy(s => s.DueDate)
                    .ThenBy(s => s.InstallmentNumber)
                    .ToListAsync(ct);

                foreach (var item in dueAndUnpaidScheduleItems)
                {
                    if (paymentRemaining <= FinancialThreshold) break;

                    var interestDueForThisInstallment = item.InterestAmount - item.PaidInterest;
                    if (interestDueForThisInstallment > FinancialThreshold)
                    {
                        var amountToPayInterest = Math.Min(paymentRemaining, interestDueForThisInstallment);
                        item.PaidInterest += amountToPayInterest;
                        paymentRemaining -= amountToPayInterest;
                        totalInterestPaid += amountToPayInterest;
                    }

                    if (paymentRemaining > FinancialThreshold && (item.PaidInterest >= item.InterestAmount - FinancialThreshold))
                    {
                        var principalDueForThisInstallment = item.PrincipalAmount - item.PaidPrincipal;
                        if (principalDueForThisInstallment > FinancialThreshold)
                        {
                            var amountToPayPrincipal = Math.Min(paymentRemaining, principalDueForThisInstallment);
                            item.PaidPrincipal += amountToPayPrincipal;
                            paymentRemaining -= amountToPayPrincipal;
                            totalPrincipalPaid += amountToPayPrincipal;
                        }
                    }

                    if (item.PaidInterest >= item.InterestAmount - FinancialThreshold &&
                        item.PaidPrincipal >= item.PrincipalAmount - FinancialThreshold)
                    {
                        item.IsPaid = true;
                        item.PaidDate = DateTime.UtcNow;                }
                        _context.LoanRepaymentSchedules.Update(item);
                }

                // 3. Handle remaining payment as principal prepayment
                if (paymentRemaining > FinancialThreshold)
                {
                    decimal principalPrepaymentAmount = paymentRemaining;
                    account.OutstandingBalance -= principalPrepaymentAmount;
                    totalPrincipalPaid += principalPrepaymentAmount;
                    _logger.LogInformation("Applied prepayment of {Amount} to principal for account {AccountId}. New outstanding balance: {Balance}.",
                        principalPrepaymentAmount, account.AccountId, account.OutstandingBalance);
                }

                // Ensure outstanding balance does not go negative
                account.OutstandingBalance = Math.Max(0, account.OutstandingBalance);

                // 4. Re-generate the remaining schedule
                if (account.OutstandingBalance > FinancialThreshold)
                {
                    var firstUnpaidScheduleItem = await _context.LoanRepaymentSchedules
                        .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid)
                        .OrderBy(s => s.DueDate)
                        .FirstOrDefaultAsync(ct);

                    var recalculationStartDate = firstUnpaidScheduleItem?.DueDate.Date ??
                        GetNextDueDate(today, (PaymentFrequency)account.PaymentFrequency);

                    await _repaymentScheduleService.GenerateAndSaveScheduleAsync(
                        accountId: account.AccountId,
                        isRecalculation: true,
                        recalculationStartDate: recalculationStartDate,
                        ct: ct);
                    _logger.LogInformation("Loan schedule re-generated for account {AccountId}.", account.AccountId);
                }
                else
                {
                    var remainingUnpaidScheduleItems = await _context.LoanRepaymentSchedules
                        .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid)
                        .ToListAsync(ct);

                    if (remainingUnpaidScheduleItems.Any())
                    {
                        _context.LoanRepaymentSchedules.RemoveRange(remainingUnpaidScheduleItems);
                        _logger.LogInformation("Removed {Count} remaining unpaid schedule items for account {AccountId} as loan is paid off.",
                            remainingUnpaidScheduleItems.Count, account.AccountId);
                    }
                }

                // 5. Update Account details
                account.UpdatedAt = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(3));

                var nextUnpaid = await _context.LoanRepaymentSchedules
                    .AsNoTracking()
                    .Where(s => s.AccountId == loanPaymentDto.AccountId && !s.IsPaid)
                    .OrderBy(s => s.DueDate)
                    .FirstOrDefaultAsync(ct);

                account.NextPaymentDate = nextUnpaid?.DueDate ?? null;

                _context.Accounts.Update(account);

                // 6. Record the transaction
                var transaction = new Transactions
                {
                    AccountId = loanPaymentDto.AccountId,
                    Amount = loanPaymentDto.Amount,
                    PrincipalAmount = Math.Round(totalPrincipalPaid, 2),
                    InterestAmount = Math.Round(totalInterestPaid, 2),
                    PenaltyAmount = Math.Round(totalPenaltiesPaid, 2),
                    TransactionDate = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(3)),
                    TransactionType = (int)TransactionType.Payment,
                    PaymentMethod = loanPaymentDto.PaymentMethod,
                };
                await _context.Transactions.AddAsync(transaction, ct);

                // 7. Close the loan if paid off
                bool loanClosed = false;
                if (account.OutstandingBalance <= FinancialThreshold)
                {
                    account.Status = (int)AccountStatus.Closed;
                    var application = await _context.LoanApplications
                        .AsTracking()
                        .FirstOrDefaultAsync(x => x.ApplicationId == account.ApplicationId, ct);

                    if (application != null)
                    {
                        application.Status = (int)LoanStatus.Closed;
                        application.UpdatedAt = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(3));
                    }
                    loanClosed = true;
                }

                // 8. Save all changes
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("Payment of {Amount} processed for account {AccountId}. Principal: {Principal}, Interest: {Interest}, Penalty: {Penalty}, New balance: {Balance}.",
                    loanPaymentDto.Amount, account.AccountId, totalPrincipalPaid, totalInterestPaid, totalPenaltiesPaid, account.OutstandingBalance);

                return new PaymentResult(
                    Success: true,
                    AmountReceived: loanPaymentDto.Amount,
                    PrincipalPaid: Math.Round(totalPrincipalPaid, 2),
                    InterestPaid: Math.Round(totalInterestPaid, 2),
                    NewOutstandingBalance: Math.Round(account.OutstandingBalance, 2),
                    NextPaymentDate: account.NextPaymentDate,
                    LoanClosed: loanClosed
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
                    .AnyAsync(p => p.AccountId == account.AccountId && p.Amount > financialThreshold, ct); // Check actual amount remaining

                var hasUnpaidSchedules = await _context.LoanRepaymentSchedules
                    .AnyAsync(s => s.AccountId == account.AccountId && !s.IsPaid &&
                                   (s.PrincipalAmount - s.PaidPrincipal > financialThreshold ||
                                    s.InterestAmount - s.PaidInterest > financialThreshold), ct); // Check if actual amount due

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
        }
    }












            
           
    
        



       
        









        





    









