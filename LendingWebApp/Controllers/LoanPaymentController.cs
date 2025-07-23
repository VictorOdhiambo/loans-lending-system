using DocumentFormat.OpenXml.Office2010.Excel;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;

namespace LoanApplicationService.Web.Controllers
{
    public class LoanPaymentController(IAccountService accountService, ILoanPaymentService loanPaymentService) : Controller

    {
        private readonly IAccountService _accountService = accountService;
        private readonly ILoanPaymentService _loanPaymentService = loanPaymentService;
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MakePayment(int Id)
        {
            var account = await _accountService.GetAccountByIdAsync(Id);

            if (account == null)
            {
                return View("NotFound");
            }

            PopulatePaymentMethods();


            var model = new LoanPaymentDto
            {
                AccountId = account.AccountId
            };

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> MakePayment(LoanPaymentDto dto)
        {
            var account = await _accountService.GetAccountByIdAsync(dto.AccountId);
            if (dto.Amount > account.OutstandingBalance)
            {
                ModelState.AddModelError("", $"The Payment amount cannot be greater than your Outstanding Balance , which is {account.OutstandingBalance:C}.");
                PopulatePaymentMethods();

                return View(dto);

            }
            if (ModelState.IsValid)
            {
                var result = await _loanPaymentService.MakePaymentAsync(dto.AccountId, dto);
                if (result)
                {
                    return RedirectToAction("GetAccountById", "Account", new { id = dto.AccountId });
                }
                ModelState.AddModelError("", "Failed to make payment.");
            }
            PopulatePaymentMethods();

            return View(dto);
        }


        private void PopulatePaymentMethods()
        {
            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();
        }

    }
}
