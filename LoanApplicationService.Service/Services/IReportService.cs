using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.LoanPortFolioOverview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoanApplicationService.Service.DTOs.LoanPortFolioOverview.LoanPortFolioOverview;


namespace LoanApplicationService.Service.Services
{
    public interface IReportService
    {
        Task<LoanPortFolioOverview> GetLoanPortfolioOverviewAsync();

        Task<List<RiskDistribution>> GetRiskDistribution();





    }
}
