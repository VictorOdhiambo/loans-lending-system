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
    [Authorize]
    public class AccountController : Controller
    public class AccountController(IAccountService accountService) : Controller
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly INotificationSenderService _notificationService;

        public AccountController(IAccountService accountService, IUserService userService, INotificationSenderService notificationService)
        {
            _accountService = accountService;
            _userService = userService;
            _notificationService = notificationService;
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return View(accounts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
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
        [Authorize(Roles = "Admin,SuperAdmin")]
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
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAccountsByUserId()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Error"] = "User is not logged in or session expired";
                return RedirectToAction("Index", "Home");
            }
            
            if (!Guid.TryParse(userIdString, out var userId))
            {
                TempData["Error"] = "Invalid user ID";
                return RedirectToAction("Index", "Home");
            }

            var accounts = await _accountService.GetAccountByUserId(userId);
            if (accounts == null || !accounts.Any())
            {
                return View("NotFound");
            }
            return View(accounts);
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetPenalties (int accountId)
        {
            // Get the account first to verify ownership
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return View("NotFound");
            }

            // Verify the customer owns this account
            var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
            if (customerService != null)
            {
                var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                if (currentCustomer == null || currentCustomer.CustomerId != account.CustomerId)
                {
                    return RedirectToAction("AccessDenied", "Home");
                }
            }

            var penalties = await _accountService.GetAccountPenalties(accountId);
            if (penalties == null || !penalties.Any())
            {
                return View("NotFound");
            }
            return View(penalties);
        }








    }
}



