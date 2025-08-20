using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using X.PagedList.Extensions;

namespace LoanApplicationService.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IRepaymentScheduleService _repaymentScheduleService;
        private readonly IUserService _userService;
        private readonly INotificationSenderService _notificationService;

        public AccountController(IAccountService accountService, IUserService userService, INotificationSenderService notificationService, IRepaymentScheduleService repaymentScheduleService)
        {
            _accountService = accountService;
            _userService = userService;
            _notificationService = notificationService;
            _repaymentScheduleService = repaymentScheduleService;
        }
       
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Index(int ? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;     
            var accounts = await _accountService.GetAllAccountsAsync();

            var pagedAccountsList = accounts.ToPagedList(pageNumber, pageSize);

            return View(pagedAccountsList);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Customer")]
        public async Task<IActionResult> GetAccountById(int Id)
        {
            var account = await _accountService.GetAccountByIdAsync(Id);
            if (account == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }

            // If user is Customer, verify they own this account
            if (User.IsInRole("Customer"))
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != account.CustomerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (userRole == "Customer")
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != account.CustomerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }
            return View(account);
        }

        // Add the missing GetAccountDetails method that the URL is trying to access
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Customer")]
        public async Task<IActionResult> GetAccountDetails(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }

            // If user is Customer, verify they own this account
            if (User.IsInRole("Customer"))
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != account.CustomerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }

            return View("GetAccountById", account);
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
                // Return the NotFound view from Shared folder
                return View("~/Views/Shared/NotFound.cshtml");
            }
            int pageSize = 10;
            int pageNumber = 1; 

            var pagedAccountList = accounts.ToPagedList(pageNumber, pageSize);
            return View(pagedAccountList);
        }

        [HttpGet]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> GetPenalties (int accountId)
        {
            // Get the account first to verify ownership
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }

            // Verify the customer owns this account (only for Customer role)
            if (User.IsInRole("Customer"))
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != account.CustomerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }

            var penalties = await _accountService.GetAccountPenalties(accountId);
            if (penalties == null || !penalties.Any())
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }
            return View(penalties);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userService.ForgotPasswordAsync(model);
            if (result)
            {
                // Send password reset email
                var user = await _userService.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userService.GeneratePasswordResetTokenAsync(model.Email);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var resetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);
                        
                        var emailData = new Dictionary<string, string>
                        {
                            ["Email"] = model.Email,
                            ["ResetLink"] = resetLink,
                            ["UserName"] = user.Username ?? "User"
                        };

                        await _notificationService.SendNotificationAsync(
                            "Password Reset",
                            "email",
                            emailData
                        );
                    }
                }

                TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent to your email address.";
                return RedirectToAction("ForgotPassword");
            }

            // Don't reveal that the user does not exist or is not confirmed
            TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent to your email address.";
            return RedirectToAction("ForgotPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordDto
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userService.ResetPasswordAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully. You can now log in with your new password.";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid reset token or email address.");
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> GetScheduleByAccount (int Id)
        {
            // Get the account first to verify ownership
            var account = await _accountService.GetAccountByIdAsync(Id);
            if (account == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }

            // Verify the customer owns this account (only for Customer role)
            if (User.IsInRole("Customer"))
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != account.CustomerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }

            var schedules = await _repaymentScheduleService.GetScheduleByAccount(Id);
            if (schedules == null || !schedules.Any())
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }
            if (account == null)
            {
                return View("NotFound");
            }
           
            return View(schedules);
        }


    }
}



