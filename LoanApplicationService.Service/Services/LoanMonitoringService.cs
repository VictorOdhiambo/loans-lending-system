using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
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
            await CheckOverduePaymentsAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    public async Task CheckOverduePaymentsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<LoanMonitoringService>>();
        try
        {
            var overdueAccounts = await context.Accounts
                .Where(a => a.Status == (int)AccountStatus.Active && a.OutstandingBalance > 0)
                .Include(a => a.RepaymentSchedules)
                .ToListAsync();
            foreach (var account in overdueAccounts)
            {
                var overduePayments = account.RepaymentSchedules
                    .Where(s => !s.IsPaid && s.DueDate < DateTime.UtcNow)
                    .ToList();
                if (overduePayments.Any())
                {
                    var overdueDays = account.RepaymentSchedules
                          .Where(s => !s.IsPaid && s.DueDate < DateTime.UtcNow)
                          .Select(s => (DateTime.UtcNow - s.DueDate).Days)
                          .Max();

                    logger.LogWarning($"Account {account.AccountId} has overdue payments. Overdue days: {overdueDays}");
                    var GracePeriod = 15;
                    if (overdueDays > GracePeriod)
                    {
                        var lastPenaltyDate = account.LastPenaltyAppliedDate;
                        int newPenaltyDays = (DateTimeOffset.UtcNow - lastPenaltyDate).Days;
                        decimal dailyPenaltyRate = 0.05m;
                        var penaltyAmount = account.OutstandingBalance * dailyPenaltyRate * newPenaltyDays;
                        var LoanPenalty = new LoanPenalty
                        {
                            AccountId = account.AccountId,
                            Amount = penaltyAmount,
                            AppliedDate = DateTime.UtcNow,
                            IsPaid = false
                        };
                        context.Add(LoanPenalty);

                        account.OutstandingBalance += penaltyAmount;
                        account.LastPenaltyAppliedDate = DateTimeOffset.UtcNow;
                        if (overdueDays > 30)
                        {
                            account.Status = (int)AccountStatus.Defaulted;
                            context.Accounts.Update(account);
                            await context.SaveChangesAsync();
                            logger.LogWarning($"Account {account.AccountId} has been marked as defaulted due to overdue payments exceeding 30 days.");
                        }
                        else
                        {
                            logger.LogInformation($"Penalty of {penaltyAmount:C} applied to account {account.AccountId} for overdue payments.");
                            context.Accounts.Update(account);
                            await context.SaveChangesAsync();



                        }



                    }
                    else
                    {
                        logger.LogInformation($"Account {account.AccountId} has no overdue payments.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking overdue payments.");
        }
    }
}
}