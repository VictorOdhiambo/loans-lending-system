using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class LoanMonitoringService : BackgroundService
{
    private readonly ILogger<LoanMonitoringService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24); 

    public LoanMonitoringService(ILogger<LoanMonitoringService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckOverdueAndPrepayments();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CheckOverdueAndPrepayments()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();

        var today = DateTime.UtcNow;

        var accounts = await dbContext.Accounts
            .Include(a => a.LoanApplication)
            .Include(a => a.Notifications)
            .Include(a => a.LoanPayments)
            .ToListAsync();

        foreach (var account in accounts)
        {
            if (account.NextPaymentDate < today && account.Status == "Active")
            {
                var lastPenaltyDate = account.LastPenaltyAppliedDate ?? account.NextPaymentDate;

                int overdueDays = (today - lastPenaltyDate).Days;

                if (overdueDays > 0)
                {
                    var OverduePenaltyAmount = account.OutstandingBalance * 0.01m * overdueDays;
                    account.OutstandingBalance += OverduePenaltyAmount;
                    account.UpdatedAt = DateTime.UtcNow;
                    account.LastPenaltyAppliedDate = today;

                    _logger.LogInformation($"Account {account.AccountId} is overdue. Penalty of {OverduePenaltyAmount} applied for {overdueDays} day(s).");

                    if ((today - account.NextPaymentDate).Days > 30)
                    {
                        account.Status = "Defaulted";
                        account.UpdatedAt = DateTime.UtcNow;
                        _logger.LogInformation($"Account {account.AccountId} marked as Defaulted.");
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        



        decimal prepaymentThresholdMultiplier = 1.5m;





            var prepayments = await dbContext.LoanPayment
            .Where(p =>
            p.AccountId == account.AccountId &&
            p.PaymentDate < account.NextPaymentDate &&
            !p.IsPrepaymentPenaltyApplied)
            .ToListAsync();

            foreach (var payment in prepayments)
            {
                if (payment.Amount > account.MonthlyPayment * 1.5m)
                {
                    decimal penalty = payment.Amount * 0.02m;

                    _logger.LogInformation($"Prepayment of {payment.Amount - account.MonthlyPayment} detected for Account {account.AccountNumber}. Penalty: {penalty}.");

                    account.OutstandingBalance += penalty;
                    account.UpdatedAt = DateTime.UtcNow;
                    payment.IsPrepaymentPenaltyApplied = true;

                    await dbContext.SaveChangesAsync();
                }
            }


        }
    }

    private async Task UpdateNextPaymentDate(Account account)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();

       var accounts = await dbContext.Accounts
            .ToListAsync();

        foreach (var thisAccount in accounts) {
            if (thisAccount != null)
            {
                if (thisAccount.NextPaymentDate == thisAccount.MaturityDate)
                {
                    _logger.LogInformation($"Account {thisAccount.AccountId} has reached maturity date. No further payments required.");
                    continue;
                }

                if (thisAccount.NextPaymentDate <= DateTime.UtcNow)
                {
                    thisAccount.NextPaymentDate = thisAccount.NextPaymentDate.AddMonths(1);
                    thisAccount.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Next payment date updated for Account {thisAccount.AccountId} to {thisAccount.NextPaymentDate}.");
                    await dbContext.SaveChangesAsync();

                }
                else
                {
                    _logger.LogInformation($"Next payment date for Account {thisAccount.AccountId} is still in the future: {thisAccount.NextPaymentDate}.");
                } 
            }
            else
            {
                _logger.LogWarning($"Account {account.AccountId} not found for next payment date update.");
            }
        }
    }

}



