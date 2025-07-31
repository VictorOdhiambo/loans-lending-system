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

        [ValidateModel]
        [HttpPost]
        public async Task<IActionResult> CreateAccount(AccountDto accountDto)
        {

            var result = await _accountService.CreateAccountAsync(accountDto);
            if (result)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Failed to create account.");

            return View(accountDto);
        }


        


        
    }
}



