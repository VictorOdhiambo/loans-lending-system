using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationService.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly RepaymentServiceImpl _repaymentService;

        public AccountController(RepaymentServiceImpl repaymentService)
        {
            _repaymentService = repaymentService;
        }

        [HttpGet]
        public IActionResult RecordRepayment(int accountId)
        {
            ViewBag.AccountId = accountId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecordRepayment(int accountId, decimal amount, string? notes)
        {
            var result = await _repaymentService.RecordRepayment(accountId, amount, notes);
            if (result)
            {
                TempData["Success"] = "Repayment recorded successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to record repayment.";
            }
            return RedirectToAction("Details", new { id = accountId });
        }
    }
}
