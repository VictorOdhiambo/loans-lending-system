using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.RepaymentSchedule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoanApplicationService.Service.Services.LoanRepaymentScheduleService;

namespace LoanApplicationService.Service.Services
{
    public class LoanRepaymentScheduleService : IRepaymentScheduleService
    {
        private readonly LoanApplicationServiceDbContext _dbContext;
        private readonly ILogger<IRepaymentScheduleService> _logger;
        private readonly IMapper _mapper;
        public LoanRepaymentScheduleService(LoanApplicationServiceDbContext dbContext, ILogger<IRepaymentScheduleService>logger, IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
        }




        public async Task<List<LoanRepaymentSchedule>> GenerateAndSaveScheduleAsync(int accountId, CancellationToken ct = default)
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

            if (account.DisbursementDate == null || account.PrincipalAmount == 0)
            {
                _logger.LogError("Disbursement date or principal amount is required for account {AccountId}.", accountId);
                throw new InvalidOperationException("Account disbursement date and principal amount are required.");
            }

            var paymentFrequency = (PaymentFrequency)account.PaymentFrequency;

            var schedule = new List<LoanRepaymentSchedule>();
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

            int numberOfRemainingPayments = totalOriginalPayments;

            DateTime firstNewInstallmentStartDate = account.DisbursementDate.Value.Date;
            DateTime currentDueDate = GetNextDueDate(firstNewInstallmentStartDate, paymentFrequency);

            _logger.LogInformation("Initial schedule for account {AccountId} starts with due date {DueDate}.", accountId, currentDueDate);

            if (numberOfRemainingPayments <= 0)
            {
                _dbContext.LoanRepaymentSchedules.AddRange(schedule);
                return schedule;
            }

            decimal totalLoanInterest = Math.Round(account.PrincipalAmount * (annualInterestRate * account.TermMonths / 12m), 2, MidpointRounding.AwayFromZero);
            decimal fixedPrincipalPerInstallment = Math.Round(account.PrincipalAmount / totalOriginalPayments, 2);
            decimal fixedInterestPerInstallment = Math.Round(totalLoanInterest / totalOriginalPayments, 2);
            decimal totalPrincipalRounded = fixedPrincipalPerInstallment * totalOriginalPayments;
            decimal totalInterestRounded = fixedInterestPerInstallment * totalOriginalPayments;
            decimal principalRoundingDifference = account.PrincipalAmount - totalPrincipalRounded;
            decimal interestRoundingDifference = totalLoanInterest - totalInterestRounded;

            // CORRECTED LINE: Initialize the previousEndDate to the day BEFORE the first installment's start date.
            DateTime previousEndDate = firstNewInstallmentStartDate.AddDays(-1);

            for (int i = 1; i <= numberOfRemainingPayments; i++)
            {
                decimal principalForPeriod = fixedPrincipalPerInstallment;
                decimal interestForPeriod = fixedInterestPerInstallment;

                if (i == numberOfRemainingPayments)
                {
                    principalForPeriod += principalRoundingDifference;
                    interestForPeriod += interestRoundingDifference;
                }

                DateTime periodStartDate = previousEndDate.AddDays(1);
                DateTime periodEndDate = currentDueDate;

                schedule.Add(new LoanRepaymentSchedule
                {
                    AccountId = account.AccountId,
                    InstallmentNumber = i,
                    DueDate = currentDueDate,
                    StartDate = periodStartDate,
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
                    previousEndDate = periodEndDate;
                    currentDueDate = GetNextDueDate(currentDueDate, paymentFrequency);
                }
            }

            _dbContext.LoanRepaymentSchedules.AddRange(schedule);
            _logger.LogInformation("Generated {Count} schedule items for account {AccountId}. First due date: {DueDate}.",
                schedule.Count, accountId, currentDueDate);

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
            // This helper method is no longer used in the new logic but is kept for completeness.
            return frequency switch
            {
                PaymentFrequency.Monthly => 30,
                PaymentFrequency.Biweekly => 14,
                PaymentFrequency.Weekly => 7,
                PaymentFrequency.Quarterly => 90,
                _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Unsupported payment frequency."),
            };
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

        public async Task <IEnumerable<RepaymentScheduleDto>> GetScheduleByAccount (int accountId)
        {
            var repaymentSchedule = await _dbContext.LoanRepaymentSchedules
                .Where(s => s.AccountId == accountId)
                .OrderBy(s => s.DueDate)
                .ToListAsync();
            var schedule = _mapper.Map<IEnumerable<RepaymentScheduleDto>>(repaymentSchedule);
            return schedule;


        }
    }

}
