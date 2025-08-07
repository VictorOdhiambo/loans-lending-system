using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static LoanApplicationService.Service.DTOs.LoanPortFolioOverview.LoanPortFolioOverview;

namespace LoanApplicationService.Web.Controllers
{
    public class ReportsController(IReportService reportService) : Controller
    {
        private readonly IReportService _reportService = reportService;
        [HttpGet]
        public async Task<IActionResult> PortfolioOverview()
        {
            var PortfolioOverview = await _reportService.GetLoanPortfolioOverviewAsync();
            var labels = PortfolioOverview.CustomerRiskDistribution
                .Select(rd => {
                    if (Enum.IsDefined(typeof(LoanRiskLevel), rd.RiskLevel))
                    {
                        var level = (LoanRiskLevel)rd.RiskLevel;
                        return EnumHelper.GetDescription(level);        
                    }
                    return $"Level {rd.RiskLevel}";
                })
                .ToArray();

            var data = PortfolioOverview.CustomerRiskDistribution.Select(rd => rd.Count).ToArray();

            ViewBag.RiskLabels = JsonConvert.SerializeObject(labels);
            ViewBag.RiskData = JsonConvert.SerializeObject(data);

            return View(PortfolioOverview);

        }

        
    }
}
