using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanPortFolioOverview;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoanApplicationService.Service.DTOs.LoanPortFolioOverview.LoanPortFolioOverview;


namespace LoanApplicationService.Service.Services
{
    public class ReportServiceImpl(LoanApplicationServiceDbContext context) : IReportService
    {
        private readonly LoanApplicationServiceDbContext _context = context;
        public async Task<LoanPortFolioOverview> GetLoanPortfolioOverviewAsync()
        {

            var schedules = await _context.LoanRepaymentSchedules
                .Where(x => !x.IsPaid)
                .ToListAsync();
            var customers = await _context.Customers.ToListAsync();
            var penalties = await _context.LoanPenalties.ToListAsync();
            var accounts = await _context.Accounts.ToListAsync();
            var payments = await _context.Transactions
                .Where(t => t.TransactionType == (int)TransactionType.Payment)
                .ToListAsync();
            var applcations = await _context.LoanApplications
                .ToListAsync();

            var monthlyDisbursements = accounts
    .Where(a => a.DisbursementDate.HasValue)
    .GroupBy(a => a.DisbursementDate.Value.ToString("yyyy-MM"))
    .Select(g => new LoanPortFolioOverview.MonthlyAmount
    {
        Month = g.Key,
        Amount = g.Sum(a => a.PrincipalAmount)
    })
    .OrderBy(m => m.Month)
    .ToList();

            var monthlyPayments = payments
    .GroupBy(s => s.TransactionDate.ToString("yyyy-MM"))
    .Select(g => new LoanPortFolioOverview.MonthlyAmount
    {
        Month = g.Key,
        Amount = g.Sum(s => s.Amount)
    })
    .OrderBy(m => m.Month)
    .ToList();





            var model = new LoanPortFolioOverview
            {
                TotalOutstanding = schedules.Sum(x => (x.PrincipalAmount - x.PaidPrincipal) + (x.InterestAmount - x.PaidInterest))
                                    + penalties.Sum(p => p.Amount),

                PrincipalOutstanding = schedules.Sum(x => x.PrincipalAmount - x.PaidPrincipal),
                InterestOutstanding = schedules.Sum(x => x.InterestAmount - x.PaidInterest),
                PenaltyOutstanding = penalties.Where(p => !p.IsPaid).Sum(p => p.Amount),



                OpenLoans = accounts.Count(a => a.Status == (int)AccountStatus.Active),
                ProcessingLoans = applcations.Count(a => a.Status == (int)LoanStatus.Pending),
                DefaultedLoans = accounts.Count(a => a.Status == (int)AccountStatus.Defaulted),
                DeniedLoans = applcations.Count(a => a.Status == (int)LoanStatus.Rejected),
                FullyPaidLoans = accounts.Count(a => a.OutstandingBalance <= 0),
                Disbursed = accounts.Sum(a => a.PrincipalAmount),
                Paid = schedules.Sum(x => x.PaidPrincipal + x.PaidInterest),
                NumberOfCustomers = customers.Count(),
                CustomerRiskDistribution = await GetRiskDistribution(),
                MonthlyDisbursements = monthlyDisbursements,
                MonthlyPayments = monthlyPayments
            };
            return model;
        }

        public async Task<List<RiskDistribution>> GetRiskDistribution()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            var riskLevels = await _context.Customers
                .GroupBy(c => c.RiskLevel)
                .Select(g => new RiskDistribution
                {
                    RiskLevel = (int)g.Key, 
                    Count = g.Count(),
                    Percentage = (int)g.Count() / totalCustomers * 100
                })
                .ToListAsync();

            return riskLevels;
        }


    }
}



