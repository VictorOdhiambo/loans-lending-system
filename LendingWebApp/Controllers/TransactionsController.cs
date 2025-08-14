using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using LoanApplicationService.Service.DTOs.LoanPayment;
using LoanApplicationService.Service.DTOs.Transactions;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace LoanApplicationService.Web.Controllers
{
    [Authorize]
    public class TransactionsController (IAccountService accountService, ILoanPaymentService loanPaymentService, ILoanWithdrawalService loanWithdrawalService): Controller
    {
        private readonly IAccountService _accountService = accountService;
        private readonly ILoanPaymentService _loanPaymentService = loanPaymentService;
        private readonly ILoanWithdrawalService _loanWithdrawalService = loanWithdrawalService;
        
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> LoanRepayment(int Id)
        {
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

            PopulatePaymentMethods();

            var model = new LoanPaymentDto
            {
                AccountId = account.AccountId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> LoanRePayment(LoanPaymentDto loanPaymentDto)
        {
            var account = await _accountService.GetAccountByIdAsync(loanPaymentDto.AccountId);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userIpAddress = GetUserIpAddress();

            if (loanPaymentDto.Amount > account.OutstandingBalance)
            {
                ModelState.AddModelError("", $"The Payment amount cannot be greater than your Outstanding Balance , which is {account.OutstandingBalance:C}.");
                PopulatePaymentMethods();

                return View(loanPaymentDto);

            }
           
                var result = await _loanPaymentService.MakePaymentAsync(loanPaymentDto, userId, userIpAddress);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Payment completed successfully!";
                    return RedirectToAction("GetTransactionsByAccountId", "Transactions", new { id = loanPaymentDto.AccountId });
                }
                ModelState.AddModelError("", "Failed to make payment.");
                PopulatePaymentMethods();
                return View(loanPaymentDto);


        }


        

        [HttpPost]
       [ValidateModel]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Withdraw(LoanWithdawalDto loanWithdawalDto ,Guid userId, string userIpAddress)
        {
            var loanAccount = await _accountService.GetAccountByIdAsync(loanWithdawalDto.AccountId);
            var myUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var ipAddress = GetUserIpAddress();


            if (loanAccount == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }

            if (loanWithdawalDto.Amount > loanAccount.AvailableBalance)
            {
                ModelState.AddModelError("Amount", $"Payment amount exceeds available balance. The available balance is {loanAccount.AvailableBalance}");
                GetPaymentMethods();

                return View(loanWithdawalDto);
            }
            var result = await _loanWithdrawalService.WithdrawAsync(loanWithdawalDto, myUserId, ipAddress);

            if (result)
            {
                TempData["SuccessMessage"] = "Withdrawal completed successfully!";
                return RedirectToAction("GetTransactionsByAccountId", "Transactions", new { id = loanWithdawalDto.AccountId });
            }

            GetPaymentMethods();

            ModelState.AddModelError("", "Failed to withdraw amount.");
            return View(loanWithdawalDto);
        }


        [HttpGet]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> Withdraw(int Id)
        {
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

            GetPaymentMethods();

            var model = new LoanWithdawalDto
            {
                AccountId = account.AccountId
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> GetTransactionsByAccountId(int id)
        {
            // Get the account first to verify ownership
            var account = await _accountService.GetAccountByIdAsync(id);
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

            var transactions = await _loanWithdrawalService.GetAllTransactionsAsync(id);
            if (transactions == null || !transactions.Any())
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }
            
            ViewBag.AccountNumber = account.AccountNumber;
            ViewBag.AccountId = account.AccountId;
            return View(transactions);
        }                       






        private void GetPaymentMethods()
        {
            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
                .Cast<PaymentMethods>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();
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


        private string GetUserIpAddress()
        {
            string ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var ipList = ipAddress.Split(',', StringSplitOptions.RemoveEmptyEntries);
                ipAddress = ipList.FirstOrDefault()?.Trim();
            }
            else
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            if (ipAddress == "::1" || string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "127.0.0.1";
            }

            return ipAddress.Length > 45 ? ipAddress.Substring(0, 45) : ipAddress;
        }
    }
}
