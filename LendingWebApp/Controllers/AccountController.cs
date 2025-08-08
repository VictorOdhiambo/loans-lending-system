using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LoanApplicationService.Web.Controllers
{
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
        public async Task<IActionResult> GetScheduleByAccount (int Id)
        {
            var schedules = await _repaymentScheduleService.GetScheduleByAccount(Id);
            if (schedules == null || !schedules.Any())
            {
                return View("NotFound");
            }
            return View(schedules);
        }
    }
}



