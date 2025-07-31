using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
   
    public interface IRepaymentScheduleService
    {

        Task<List<LoanRepaymentSchedule>> GenerateAndSaveScheduleAsync(int accountId, bool isRecalculation = false, DateTime? recalculationStartDate = null, CancellationToken ct = default);


        Task<List<LoanRepaymentSchedule>> GetScheduleByAccountAsync(int accountId);


        Task MarkInstallmentPaidAsync(int scheduleId);

    }
}
