using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationService.Web.Controllers
{
    public class LoanChargeController : Controller
    {
        private readonly ILoanChargeService _loanChargeService;
        public IActionResult Index()
        {
            return View();
        }
    }
}
