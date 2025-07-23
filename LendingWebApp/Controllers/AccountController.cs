using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LoanApplicationService.Web.Controllers
{
    public class AccountController(IAccountService accountService) : Controller
    {
        private readonly IAccountService _accountService = accountService;
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountById(int Id)
        {
            var account = await _accountService.GetAccountByIdAsync(Id);
            if (account == null)
            {
                return View("NotFound");
            }
            return View(account);
        }

        public async Task<IActionResult> CreateAccount(AccountDto accountDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.CreateAccountAsync(accountDto);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to create account.");
            }
            return View(accountDto);
        }

        [HttpPost]
        [RoleAuthorize("Customer")]

        public async Task<IActionResult> Withdraw(LoanWithdawalDto dto)
        {
            var LoanAccount = await _accountService.GetAccountByIdAsync(dto.AccountId);
            if (dto.Amount > LoanAccount.AvailableBalance)
            {
                ModelState.AddModelError("Amount", $"Payment amount exceeds available balance. The available balance is {LoanAccount.AvailableBalance}");
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
               .Cast<PaymentMethods>()
               .Select(e => new SelectListItem
               {
                   Value = ((int)e).ToString(),
                   Text = EnumHelper.GetDescription(e)
               }).ToList();
                return View(dto);
            }
            var result = await _accountService.WithdrawAsync(dto.AccountId, dto);

            if (result)
            {
                return RedirectToAction("Index");
            }

            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();

            ModelState.AddModelError("", "Failed to withdraw amount.");
            return View(dto);
        }


        [HttpGet]
        [RoleAuthorize("Customer")]

        public async Task<IActionResult> Withdraw(int Id)
        {
            var account = await _accountService.GetAccountByIdAsync(Id);

            if (account == null)
            {
                return View("NotFound");
            }

            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();

            var model = new LoanWithdawalDto
            {
                AccountId = account.AccountId
            };

            return View(model);
        }



        [HttpPost]
        [RoleAuthorize("Customer")]

        public async Task<IActionResult> ApplyPayment(int accountId, decimal amount)
        {
            var LoanAccount = await _accountService.GetAccountByIdAsync(accountId);

            if (ModelState.IsValid)
            {

                var result = await _accountService.ApplyPaymentAsync(accountId, amount);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to apply payment.");
            }

            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> MakePayment(int accountId, decimal amount)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.ApplyPaymentAsync(accountId, amount);
                if (result)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to make payment");
            }
            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();

            return View();
        }
    }
}



