using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LoanApplicationService.Web.Controllers
{
    public class AccountController(IAccountService accountService) : Controller
    {
        private readonly IAccountService _accountService = accountService;
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
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

        [HttpGet]
        public async Task<IActionResult> GetAccountsByUserId()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == string.Empty)
            {
                TempData["Error"] = "User is not logged in or session expired";
                RedirectToAction("", "Home");
            }
            var userId = Guid.Parse(userIdString);

            var accounts = await _accountService.GetAccountByUserId(userId);
            if (accounts == null || !accounts.Any())
            {
                return View("NotFound");
            }
            return View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> GetPenalties (int accountId)
        {
            var penalties = await _accountService.GetAccountPenalties(accountId);
            if (penalties == null || !penalties.Any())
            {
                return View("NotFound");
            }
            return View(penalties);
        }








    }
}



