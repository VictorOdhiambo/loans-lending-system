using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace LoanApplicationService.Web.Controllers
{
    [Authorize]
    public class LoanApplicationController : Controller
    {
        private readonly ILoanApplicationService _loanApplicationService;
        private readonly ILoanProductService _loanProductService;
        private readonly INotificationSenderService _notificationSenderService;
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly ILoanChargeService _loanChargeService;
        private readonly IRepaymentScheduleService _loanRepaymentScheduleService;

        public LoanApplicationController(
            ILoanApplicationService loanApplicationService,
            ILoanProductService loanProductService,
            INotificationSenderService notificationSenderService,
            ICustomerService customerService,
            IAccountService accountService,
            ILoanChargeService loanChargeService,
            IRepaymentScheduleService loanRepaymentScheduleService

            )

        {
            _loanApplicationService = loanApplicationService;
            _loanProductService = loanProductService;
            _notificationSenderService = notificationSenderService;
            _customerService = customerService;
            _accountService = accountService;
            _loanChargeService = loanChargeService;
            _loanRepaymentScheduleService = loanRepaymentScheduleService;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Disburse(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("NotFound");
            }
            return View(application);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisburseConfirmed(int id)
        {
            var result = await _loanApplicationService.DisburseAsync(id);
            if (result)
            {
                // Send notification to customer using the 'Loan Disbursed' template
                var application = await _loanApplicationService.GetByIdAsync(id);
                if (application != null)
                {
                    var customer = await _customerService.GetByIdAsync(application.CustomerId);
                    if (customer != null)
                    {
                        var data = new Dictionary<string, string>
                        {
                            ["FirstName"] = customer.FirstName,
                            ["LastName"] = customer.LastName,
                            ["FullName"] = customer.FirstName + " " + customer.LastName,
                            ["Email"] = customer.Email,
                            ["LoanProductId"] = application.ProductId.ToString(),
                            ["LoanAmount"] = (application.ApprovedAmount > 0 ? application.ApprovedAmount : application.RequestedAmount).ToString("F2"),
                            ["TermMonths"] = application.TermMonths.ToString(),
                            ["Purpose"] = application.Purpose ?? string.Empty
                        };
                        await _notificationSenderService.SendNotificationAsync(
                            "Loan Disbursed",
                            "email",
                            data
                        );
                    }
                }
                TempData["LoanApplicationSuccess"] = "Loan application disbursed successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to disburse loan application.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? Status, int? customerId)
        {
            // If customerId is provided, ensure the user is either an admin/superadmin or the customer themselves
            if (customerId.HasValue)
            {
                if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
                {
                    // For customers, verify they can only see their own applications
                    var customer = await _customerService.GetByIdAsync(customerId.Value);
                    if (customer == null || customer.Email != User.Identity.Name)
                    {
                        return Forbid();
                    }
                }
            }
            else
            {
                // If no customerId provided, handle based on user role
                if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin"))
                {
                    // Admins and SuperAdmins can see all applications when no customerId is provided
                    customerId = null;
                }
                else if (User.IsInRole("Customer"))
                {
                    // For customers, automatically get their customer ID
                    var currentCustomer = await _customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer != null)
                    {
                        customerId = currentCustomer.CustomerId;
                    }
                    else
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }

            var applications = await _loanApplicationService.GetAllAsync();

            if (Status.HasValue)
            {
                applications = applications.Where(a => a.Status == (LoanStatus)Status.Value);
            }
            if (customerId.HasValue)
            {
                applications = applications.Where(a => a.CustomerId == customerId.Value);
                ViewBag.CustomerId = customerId.Value;
                // Fetch customer full name
                var customer = await _customerService.GetByIdAsync(customerId.Value);
                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FirstName + " " + customer.LastName;
                }
            }

            var statusList = Enum.GetValues(typeof(LoanStatus))
                .Cast<LoanStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString(),
                    Selected = Status.HasValue && (int)s == Status.Value
                }).ToList();

            ViewBag.StatusList = statusList;
            ViewBag.SelectedStatus = Status;

            return View(applications);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("NotFound");
            }
            return View(application);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? customerId, int? productId)
        {
            await PopulateLoanProductsDropdown();

            string customerName = null;
            int actualCustomerId = 0;

            // If customerId is provided, use it (for admin/super admin creating for specific customer)
            if (customerId.HasValue)
            {
                var customer = await _customerService.GetByIdAsync(customerId.Value);
                if (customer != null)
                {
                    customerName = customer.FirstName + " " + customer.LastName;
                    ViewBag.CustomerName = customerName;
                    ViewBag.CustomerId = customer.CustomerId;
                    actualCustomerId = customer.CustomerId;
                }
            }
            // If no customerId but user is a customer, get their own information
            else if (User.IsInRole("Customer"))
            {
                var customer = await _customerService.GetByEmailAsync(User.Identity.Name);
                if (customer != null)
                {
                    customerName = customer.FirstName + " " + customer.LastName;
                    ViewBag.CustomerName = customerName;
                    ViewBag.CustomerId = customer.CustomerId;
                    actualCustomerId = customer.CustomerId;
                }
            }

            var loanApplicationDto = new LoanApplicationDto();
            if (customerId.HasValue) loanApplicationDto.CustomerId = customerId.Value;
            if (productId.HasValue) loanApplicationDto.ProductId = productId.Value;

            return View(loanApplicationDto);
        }




        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Create(LoanApplicationDto loanApplicationDto)
        {
            var applicantCustomer = await _customerService.GetByIdAsync(loanApplicationDto.CustomerId);
            if (applicantCustomer == null)
            {
                ModelState.AddModelError("CustomerId", "Invalid customer selected.");
                await PopulateLoanProductsDropdown();

                return View(loanApplicationDto);
            }

            // Fetch the selected loan product
            var product = await _loanProductService.GetLoanProductById(loanApplicationDto.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Invalid loan product selected.");
                await PopulateLoanProductsDropdown();

                return View(loanApplicationDto);
            }
            else
            {
                if (loanApplicationDto.TermMonths < product.MinTermMonths || loanApplicationDto.TermMonths > product.MaxTermMonths)
                {
                    ModelState.AddModelError("TermMonths", $"Term must be between {product.MinTermMonths} and {product.MaxTermMonths} months.");
                }
                if (loanApplicationDto.RequestedAmount < product.MinAmount || loanApplicationDto.RequestedAmount > product.MaxAmount)
                {
                    ModelState.AddModelError("RequestedAmount", $"Requested amount must be between {product.MinAmount} and {product.MaxAmount}.");
                }
            }



            //get user or customer Id
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("", "Home");
            }
loanApplicationDto.CreatedBy = Guid.Parse(userIdStr ?? string.Empty);

            var result = await _loanApplicationService.CreateAsync(loanApplicationDto);
            if (result)
            {
                // Send notification to customer using the template ID (2 = Loan Application Received)
                var customer = await _customerService.GetByIdAsync(loanApplicationDto.CustomerId);
                if (customer != null)
                {
                    var data = new Dictionary<string, string>
                    {
                        ["FirstName"] = customer.FirstName,
                        ["Email"] = customer.Email,
                        ["LoanProductId"] = loanApplicationDto.ProductId.ToString(),
                        ["LoanAmount"] = loanApplicationDto.RequestedAmount.ToString("F2")
                    };
                    await _notificationSenderService.SendNotificationByTemplateId(2, data);
                }
                TempData["LoanApplicationSuccess"] = "Loan application created successfully!";
                if (loanApplicationDto.CustomerId != 0)
                {
                    return RedirectToAction("Index", new { customerId = loanApplicationDto.CustomerId });
                }
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Unexpected error occurred while saving.";
            return View(loanApplicationDto);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]

        public async Task<IActionResult> Approve(int id)
        {

            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("NotFound");
            }
            await PopulateLoanProductsDropdown();
            return View(application);

        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Approve(LoanApplicationDto loanApplicationDto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("", "Home");
            }
            var LoanApplication = await _loanApplicationService.GetByIdAsync(loanApplicationDto.ApplicationId);
            if (LoanApplication == null)
            {
                TempData["Error"] = "Loan application not found.";
                return RedirectToAction("Index");
            }

            var ApprovedBy = Guid.Parse(userIdStr);
            if (LoanApplication.CreatedBy == ApprovedBy)
            {
                ModelState.AddModelError(nameof(loanApplicationDto.ApprovedBy), "You cannot approve your own application.");
                TempData["Error"] = "You cannot approve your own application.";
                return RedirectToAction("Index");
            }

            if (loanApplicationDto.ApprovedAmount <= 0)
            {
                ModelState.AddModelError(nameof(loanApplicationDto.ApprovedAmount), "Approved Amount is required.");
                await PopulateLoanProductsDropdown();

                return View(loanApplicationDto);
            }



            await _loanApplicationService.ApproveAsync(loanApplicationDto.ApplicationId, loanApplicationDto.ApprovedAmount, ApprovedBy);
            // Send notification to customer using the custom template
            var customer = await _customerService.GetByIdAsync(loanApplicationDto.CustomerId);
            if (customer != null)
            {
                var data = new Dictionary<string, string>
                {
                    ["FirstName"] = customer.FirstName,
                    ["LastName"] = customer.LastName,
                    ["FullName"] = customer.FirstName + " " + customer.LastName,
                    ["Email"] = customer.Email,
                    ["LoanProductId"] = loanApplicationDto.ProductId.ToString(),
                    ["LoanAmount"] = (loanApplicationDto.ApprovedAmount > 0 ? loanApplicationDto.ApprovedAmount : loanApplicationDto.RequestedAmount).ToString("F2"),
                    ["TermMonths"] = loanApplicationDto.TermMonths.ToString(),
                    ["Purpose"] = loanApplicationDto.Purpose ?? string.Empty
                };
                await _notificationSenderService.SendNotificationAsync(
                    "Loan Approved",
                    "email",
                    data
                );
            }
            TempData["LoanApplicationSuccess"] = "Loan application approved successfully!";
            return RedirectToAction("Index");


        }


        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]

        public async Task<IActionResult> Reject(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("NotFound");
            }
            await PopulateLoanProductsDropdown();

            return View(application);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateModel]


        public async Task<ActionResult> RejectAsync(int applicationId)
        {

            var userIdStr = Guid.Parse(HttpContext.Session.GetString("UserId") ?? string.Empty);
            var loanApplication = await _loanApplicationService.GetByIdAsync(applicationId);
            if (loanApplication.CreatedBy == userIdStr)

            {
                TempData["Error"] = "You cannot reject your own application.";
                return RedirectToAction("GetById", new { id = applicationId });
            }

            loanApplication.RejectedBy = userIdStr;
            var result = await _loanApplicationService.RejectAsync(applicationId);
            if (result)
            {
                // Send notification to customer using the 'Loan Rejected' template
                var application = await _loanApplicationService.GetByIdAsync(applicationId);
                if (application != null)
                {
                    var customer = await _customerService.GetByIdAsync(application.CustomerId);
                    if (customer != null)
                    {
                        var data = new Dictionary<string, string>
                        {
                            ["FirstName"] = customer.FirstName,
                            ["LastName"] = customer.LastName,
                            ["FullName"] = customer.FirstName + " " + customer.LastName,
                            ["Email"] = customer.Email,
                            ["LoanProductId"] = application.ProductId.ToString(),
                            ["LoanAmount"] = application.RequestedAmount.ToString("F2"),
                            ["TermMonths"] = application.TermMonths.ToString(),
                            ["Purpose"] = application.Purpose ?? string.Empty
                        };
                        await _notificationSenderService.SendNotificationAsync(
                            "Loan Rejected",
                            "email",
                            data
                        );
                    }
                }
                TempData["LoanApplicationSuccess"] = "Loan application rejected successfully!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Unexpected error occurred while rejecting the application.";
            return RedirectToAction("Index");

        }

        //customer views
        // GET: LoanApplication/CustomerView
        [Authorize]
        [HttpGet("/LoanApplication/CustomerView/{customerId}")]
        public async Task<IActionResult> CustomerView(int customerId)
        {
            // Ensure the user is either an admin/superadmin or the customer themselves
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            {
                // For customers, verify they can only see their own applications
                var customer = await _customerService.GetByIdAsync(customerId);
                if (customer == null || customer.Email != User.Identity.Name)
                {
                    return Forbid();
                }
            }

            var applications = await _loanApplicationService.GetByCustomerIdAsync(customerId);
            return View(applications);
        }

        // POST: LoanApplication/CustomerReject
        [Authorize]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CustomerReject(int applicationId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Reason for rejection is required.";
                // Get the customer ID from the application
                var application = await _loanApplicationService.GetByIdAsync(applicationId);
                if (application != null)
                {
                    return RedirectToAction("CustomerView", new { customerId = application.CustomerId });
                }
                return RedirectToAction("Index");
            }
            var result = await _loanApplicationService.CustomerReject(applicationId, reason);
            if (result)
            {
                TempData["LoanApplicationSuccess"] = "Loan application rejected successfully!";
            }
            else
            {
                TempData["Error"] = "Unexpected error occurred while rejecting the application.";
            }
            // Get the customer ID from the application
            var app = await _loanApplicationService.GetByIdAsync(applicationId);
            if (app != null)
            {
                return RedirectToAction("CustomerView", new { customerId = app.CustomerId });
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CustomerReject(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);

            //check if application exists, is disbursed or  approved
            if (application != null)
            {
                return View(application);

            }

            TempData["Error"] = "Loan application not found or cannot be rejected.";
            return RedirectToAction("CustomerView", new { customerId = application.CustomerId });


        }



        [HttpPost]
        public FileResult Export()
        {

            DataTable dt = new DataTable("Loan Application");
            dt.Columns.AddRange(new DataColumn[11] { new DataColumn("ApplicationId"),
                                            new DataColumn("CustomerId"),
                                            new DataColumn("ProductId"),
                                            new DataColumn("ProcessedBy"),
                                            new DataColumn("Status"),
                                            new DataColumn("TermMonths"),
                                            new DataColumn("RequestedAmount"),
                                            new DataColumn("ApprovedAmount"),
                                            new DataColumn("Purpose"),
                                            new DataColumn("ApplicationDate"),
                                            new DataColumn("DecisionDate")});

            var LoanAplications = _loanApplicationService.GetAllAsync().Result;

            foreach (var app in LoanAplications)
            {
                dt.Rows.Add(app.ApplicationId, app.CustomerId, app.ProductId, app.ApprovedBy, app.Status, app.TermMonths, app.RequestedAmount, app.ApprovedAmount, app.Purpose, app.ApplicationDate, app.DecisionDate);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LoanApplications.xlsx");
                }
            }
        }




        [HttpGet]
        public async Task<IActionResult> DisburseLoan(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null || application.Status != LoanStatus.Approved)
            {
                TempData["Error"] = "Loan application not found or not approved.";
                return RedirectToAction("Index");
            }
            var LoanProducts = await _loanProductService.GetAllProducts();

            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            return View(application);
        }

        private bool IsUserAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        private async Task PopulateLoanProductsDropdown()
        {
            var loanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = loanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
        }





        [HttpPost]
        public async Task<IActionResult> DisburseLoanConfirmed(int applicationId)
        {
            // 1. Fetch Loan Application and Loan Product details
            // Ensure your GetByIdAsync eagerly loads the LoanProduct
            var application = await _loanApplicationService.GetByIdAsync(applicationId); // Should include LoanProduct

            if (application == null || application.Status != LoanStatus.Approved)
            {
                TempData["Error"] = "Loan application not found or not approved.";
                return RedirectToAction("Index");
            }




            // 2. Calculate Upfront Charges
            var upfrontCharges = await _loanChargeService.GetUpFrontCharges(application.ProductId);
            upfrontCharges ??= new List<LoanChargeDto>();

            decimal totalUpfrontFees = 0;
            decimal principalAmount = application.ApprovedAmount;

            foreach (var charge in upfrontCharges)
            {
                decimal chargeAmount = charge.IsPercentage ? principalAmount * charge.Amount : charge.Amount;
                totalUpfrontFees += chargeAmount;
            }

            decimal availableBalanceForDisbursement = principalAmount - totalUpfrontFees;

            // 3. Prepare Account DTO for creation
            var accountDto = new AccountDto
            {
                ApplicationId = application.ApplicationId,
                CustomerId = application.CustomerId,
                AccountNumber = "ACCT" + application.ApplicationId,
                AccountType = "Loan",
                PrincipalAmount = principalAmount,
                DisbursedAmount = availableBalanceForDisbursement,
                AvailableBalance = availableBalanceForDisbursement,
                InterestRate = application.InterestRate,
                OutstandingBalance = principalAmount,
                TermMonths = application.TermMonths,
                PaymentFrequency = application.PaymentFrequency,
                Status = (int)AccountStatus.Active,
                DisbursementDate = DateTime.UtcNow
            };

            // 4. Create the Account (this will save the account and give it an ID)
            var result = await _accountService.CreateAccountAsync(accountDto);
            if (!result)
            {
                TempData["Error"] = "Failed to create loan account.";
                return RedirectToAction("Index");
            }
            var createdAccount = await _accountService.GetAccountByApplicationIdAsync(application.ApplicationId);

            // 5. Generate the initial repayment schedule
           
            var initialSchedule = await _loanRepaymentScheduleService.GenerateAndSaveScheduleAsync(
                createdAccount.AccountId
            );

            // 6. Update Account with derived schedule info (NextPaymentDate, MaturityDate, MonthlyPayment)
            if (initialSchedule != null && initialSchedule.Any())
            {
                createdAccount.NextPaymentDate = initialSchedule.First().DueDate;
                createdAccount.MaturityDate = initialSchedule.Last().DueDate;
                // MonthlyPayment should reflect the periodic scheduled amount for this loan
                createdAccount.MonthlyPayment = initialSchedule.First().ScheduledAmount; // Assumes first payment is typical
            }
            else
            {
                // Handle edge case: loan with 0 term or special conditions resulting in no schedule
                createdAccount.NextPaymentDate = createdAccount.DisbursementDate ?? DateTime.UtcNow;
                createdAccount.MaturityDate = createdAccount.DisbursementDate ?? DateTime.UtcNow;
                createdAccount.MonthlyPayment = 0;
            }
            // Save these updates back to the account.
            await _accountService.UpdateAccountAsync(createdAccount);

            // 7. Update Loan Application Status
            application.Status = LoanStatus.Disbursed;
            await _loanApplicationService.UpdateAsync(application);

            

            TempData["Success"] = $"Loan for application {applicationId} disbursed successfully.";
            return RedirectToAction("Index");
        }
    }


}



