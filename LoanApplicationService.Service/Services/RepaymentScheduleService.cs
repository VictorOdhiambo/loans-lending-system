using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public class LoanRepaymentScheduleService : IRepaymentScheduleService
    {
        private readonly LoanApplicationServiceDbContext _dbContext;
        private readonly ILogger<IRepaymentScheduleService> _logger;
        public LoanRepaymentScheduleService(LoanApplicationServiceDbContext dbContext, ILogger<IRepaymentScheduleService>logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }



        public async Task<List<LoanRepaymentSchedule>> GenerateAndSaveScheduleAsync(
     int accountId,
     bool isRecalculation = false,
     DateTime? recalculationStartDate = null,
     CancellationToken ct = default)
        {
            const decimal FinancialThreshold = 0.01m;

            var account = await _dbContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountId == accountId, ct);

            if (account == null)
            {
                _logger.LogError("Account with ID {AccountId} not found.", accountId);
                throw new ArgumentException($"Account with ID {accountId} not found.", nameof(accountId));
            }

            if (account.DisbursementDate == null)
            {
                _logger.LogError("Disbursement date is required for account {AccountId}.", accountId);
                throw new InvalidOperationException("Account disbursement date is required.");
            }

            if (isRecalculation && recalculationStartDate.HasValue && recalculationStartDate.Value < account.DisbursementDate.Value.Date)
            {
                _logger.LogWarning("Recalculation start date {RecalculationStartDate} is before disbursement date {DisbursementDate} for account {AccountId}.",
                    recalculationStartDate.Value, account.DisbursementDate.Value, accountId);
                throw new ArgumentException("Recalculation start date cannot be before disbursement date.", nameof(recalculationStartDate));
            }

            var paymentFrequency = (PaymentFrequency)account.PaymentFrequency;

            if (isRecalculation)
            {
                var existingUnpaidSchedules = await _dbContext.LoanRepaymentSchedules
                    .Where(s => s.AccountId == accountId && !s.IsPaid)
                    .ToListAsync(ct);
                if (existingUnpaidSchedules.Any())
                {
                    _dbContext.LoanRepaymentSchedules.RemoveRange(existingUnpaidSchedules);
                    _logger.LogInformation("Removed {Count} unpaid schedules for account {AccountId} during recalculation.", existingUnpaidSchedules.Count, accountId);
                }
            }

            var schedule = new List<LoanRepaymentSchedule>();
            decimal currentLoanPrincipal = account.OutstandingBalance;
            decimal annualInterestRate = account.InterestRate;

            double GetPaymentsPerYear(PaymentFrequency frequency)
            {
                return frequency switch
                {
                    PaymentFrequency.Monthly => 12,
                    PaymentFrequency.Biweekly => 26,
                    PaymentFrequency.Weekly => 52,
                    PaymentFrequency.Quarterly => 4,
                    _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Unsupported payment frequency."),
                };
            }

            var totalOriginalPayments = (int)Math.Ceiling(account.TermMonths * (GetPaymentsPerYear(paymentFrequency) / 12d));

            int numberOfRemainingPayments;
            DateTime firstNewInstallmentDueDate;
            DateTime firstNewInstallmentStartDate;
            var businessDate = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(3)).Date; // EAT (UTC+3)

            if (isRecalculation)
            {
                var paymentsMade = await _dbContext.LoanRepaymentSchedules
                    .AsNoTracking()
                    .CountAsync(s => s.AccountId == accountId && s.IsPaid, ct);
                numberOfRemainingPayments = Math.Max(0, totalOriginalPayments - paymentsMade);

                var lastPaidSchedule = await _dbContext.LoanRepaymentSchedules
                    .AsNoTracking()
                    .Where(s => s.AccountId == accountId && s.IsPaid)
                    .OrderByDescending(s => s.InstallmentNumber)
                    .FirstOrDefaultAsync(ct);

                firstNewInstallmentStartDate = recalculationStartDate?.Date ??
                    (lastPaidSchedule?.DueDate.AddDays(1).Date ?? account.DisbursementDate.Value.Date);
                firstNewInstallmentDueDate = GetNextDueDate(firstNewInstallmentStartDate, paymentFrequency);

                if (firstNewInstallmentDueDate < businessDate)
                {
                    firstNewInstallmentDueDate = GetNextDueDate(businessDate, paymentFrequency);
                    firstNewInstallmentStartDate = firstNewInstallmentDueDate.AddDays(GetPreviousIntervalDays(paymentFrequency) * -1);
                    _logger.LogInformation("Adjusted first due date to {DueDate} for account {AccountId} as it was in the past.", firstNewInstallmentDueDate, accountId);
                }
            }
            else
            {
                numberOfRemainingPayments = totalOriginalPayments;
                firstNewInstallmentStartDate = account.DisbursementDate.Value.Date;
                firstNewInstallmentDueDate = GetNextDueDate(firstNewInstallmentStartDate, paymentFrequency);
                _logger.LogInformation("Initial schedule for account {AccountId} starts with due date {DueDate}.", accountId, firstNewInstallmentDueDate);
            }

            if (numberOfRemainingPayments <= 0 || currentLoanPrincipal <= FinancialThreshold)
            {
                _dbContext.LoanRepaymentSchedules.AddRange(schedule);
                _logger.LogInformation("No schedule generated for account {AccountId}. Remaining payments: {Payments}, Principal: {Principal}.",
                    accountId, numberOfRemainingPayments, currentLoanPrincipal);
                return schedule;
            }

            decimal periodicInterestRate = annualInterestRate / (decimal)GetPaymentsPerYear(paymentFrequency);

            decimal periodicPayment;
            if (periodicInterestRate == 0)
            {
                periodicPayment = currentLoanPrincipal / numberOfRemainingPayments;
            }
            else
            {
                if (numberOfRemainingPayments <= 0)
                {
                    periodicPayment = 0;
                }
                else
                {
                    var pow = (decimal)Math.Pow((double)(1 + periodicInterestRate), (double)numberOfRemainingPayments);
                    periodicPayment = currentLoanPrincipal * (periodicInterestRate * pow) / (pow - 1);
                }
            }
            periodicPayment = Math.Round(periodicPayment, 2, MidpointRounding.AwayFromZero);

            decimal remainingBalance = currentLoanPrincipal;
            DateTime currentDueDate = firstNewInstallmentDueDate;
            DateTime currentStartDate = firstNewInstallmentStartDate;

            for (int i = 1; i <= numberOfRemainingPayments; i++)
            {
                decimal interestForPeriod = Math.Round(remainingBalance * periodicInterestRate, 2, MidpointRounding.AwayFromZero);
                decimal principalForPeriod = Math.Round(periodicPayment - interestForPeriod, 2, MidpointRounding.AwayFromZero);

                if (i == numberOfRemainingPayments)
                {
                    principalForPeriod = remainingBalance;
                    interestForPeriod = Math.Round(remainingBalance * periodicInterestRate, 2, MidpointRounding.AwayFromZero);
                    periodicPayment = principalForPeriod + interestForPeriod;
                }
                else if (remainingBalance - principalForPeriod < FinancialThreshold && remainingBalance - principalForPeriod > -FinancialThreshold)
                {
                    principalForPeriod = remainingBalance;
                    interestForPeriod = Math.Round(remainingBalance * periodicInterestRate, 2, MidpointRounding.AwayFromZero);
                    periodicPayment = principalForPeriod + interestForPeriod;
                }

                if (principalForPeriod < 0)
                {
                    principalForPeriod = 0;
                }
                if (interestForPeriod < 0)
                {
                    principalForPeriod += interestForPeriod;
                    interestForPeriod = 0;
                }

                remainingBalance -= principalForPeriod;
                if (remainingBalance < 0) remainingBalance = 0;

                DateTime periodEndDate = currentDueDate;

                schedule.Add(new LoanRepaymentSchedule
                {
                    AccountId = account.AccountId,
                    InstallmentNumber = i,
                    DueDate = currentDueDate,
                    StartDate = currentStartDate,
                    EndDate = periodEndDate,
                    ScheduledAmount = Math.Round(principalForPeriod + interestForPeriod, 2, MidpointRounding.AwayFromZero),
                    InterestAmount = interestForPeriod,
                    PrincipalAmount = principalForPeriod,
                    IsPaid = false,
                    PaidPrincipal = 0,
                    PaidInterest = 0,
                });

                if (i < numberOfRemainingPayments)
                {
                    currentStartDate = currentDueDate.AddDays(1);
                    currentDueDate = GetNextDueDate(currentDueDate, paymentFrequency);
                }
            }

            _dbContext.LoanRepaymentSchedules.AddRange(schedule);
            _logger.LogInformation("Generated {Count} schedule items for account {AccountId}. First due date: {DueDate}.",
                schedule.Count, accountId, firstNewInstallmentDueDate);

            return schedule;
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

        private int GetPreviousIntervalDays(PaymentFrequency frequency)
        {
            return frequency switch
            {
                PaymentFrequency.Monthly => 30,
                PaymentFrequency.Biweekly => 14,
                PaymentFrequency.Weekly => 7,
                PaymentFrequency.Quarterly => 90,
                _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Unsupported payment frequency."),
            };
        }

        

        private double GetFrequencyMultiplier(PaymentFrequency frequency)
        {
            return frequency switch
            {
                PaymentFrequency.Monthly => 12d / 12d, 
                PaymentFrequency.Biweekly => 26d / 12d, 
                PaymentFrequency.Weekly => 52d / 12d,   
                PaymentFrequency.Quarterly => 4d / 12d, 
                _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Unsupported payment frequency."),
            };
        }


        
        public async Task<List<LoanRepaymentSchedule>> GetScheduleByAccountAsync(int accountId)
        {
            return await _dbContext.LoanRepaymentSchedules
                .Where(s => s.AccountId == accountId)
                .OrderBy(s => s.DueDate)
                .ToListAsync();
        }

        public async Task MarkInstallmentPaidAsync(int scheduleId)
        {
            var schedule = await _dbContext.LoanRepaymentSchedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                schedule.IsPaid = true;
                await _dbContext.SaveChangesAsync();
            }
 
        }
    }

}
