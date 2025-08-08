using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.RepaymentSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
   
    public interface IRepaymentScheduleService
    {

        Task<List<LoanRepaymentSchedule>> GenerateAndSaveScheduleAsync(int accountId, CancellationToken ct = default);



        Task MarkInstallmentPaidAsync(int scheduleId);

        Task <IEnumerable<RepaymentScheduleDto>> GetScheduleByAccount(int accountId);


    }
}
