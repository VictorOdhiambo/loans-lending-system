using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

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
        private readonly IAuditService _auditService;

        public LoanApplicationController(
            ILoanApplicationService loanApplicationService,
            ILoanProductService loanProductService,
            INotificationSenderService notificationSenderService,
            ICustomerService customerService,
            IAccountService accountService,
            ILoanChargeService loanChargeService,
            IRepaymentScheduleService loanRepaymentScheduleService,
            IAuditService auditService

            )

        {
            _loanApplicationService = loanApplicationService;
            _loanProductService = loanProductService;
            _notificationSenderService = notificationSenderService;
            _customerService = customerService;
            _accountService = accountService;
            _loanChargeService = loanChargeService;
            _loanRepaymentScheduleService = loanRepaymentScheduleService;
            _auditService = auditService;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Disburse(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
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
                            ["Purpose"] = EnumHelper.GetDescription((LoanApplicationPurpose) application.Purpose )
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
        [Authorize(Roles = "Admin,SuperAdmin,Customer")]
        public async Task<IActionResult> Index(int? Status, int? customerId, int ? page, string sortOrder = null)
        {
            var applications = await _loanApplicationService.GetAllAsync();
            var applicationsList = applications.ToList();

            // If user is Customer, they can only view their own applications
            if (User.IsInRole("Customer"))
            {
                var currentCustomer = await _customerService.GetByEmailAsync(User.Identity.Name);
                if (currentCustomer == null)
                {
                    return RedirectToAction("AccessDenied", "Home");
                }
                applicationsList = applicationsList.Where(a => a.CustomerId == currentCustomer.CustomerId).ToList();
            }

            // Filter by status if provided
            if (Status.HasValue)
            {
                applicationsList = applicationsList.Where(a => (int)a.Status == Status.Value).ToList();
            }

            // Filter by customer if provided (only for Admin/SuperAdmin)
            if (customerId.HasValue && !User.IsInRole("Customer"))
            {
                applicationsList = applicationsList.Where(a => a.CustomerId == customerId.Value).ToList();
            }


            //sort based on sort order
            switch (sortOrder)
            {
                case "ApplicationIdAsc":
                    applications = applicationsList.OrderBy(a => a.ApplicationId);
                    break;
                case "ApplicationIdDesc":
                    applications = applicationsList.OrderByDescending(a => a.ApplicationId);
                    break;
                case "CustomerNameAsc":
                    applications = applicationsList.OrderBy(a => a.FirstName + " " + a.LastName);
                    break;
                case "CustomerNameDesc":
                    applications = applicationsList.OrderByDescending(a => a.FirstName + " " + a.LastName);
                    break;
                case "ProductNameAsc":
                    applications = applicationsList.OrderBy(a => a.ProductName);
                    break;
                case "ProductNameDesc":
                    applications = applicationsList.OrderByDescending(a => a.ProductName);
                    break;
                case "StatusAsc":
                    applications = applicationsList.OrderBy(a => a.Status);
                    break;
                case "StatusDesc":
                    applications = applicationsList.OrderByDescending(a => a.Status);
                    break;
                case "TermMonthsAsc":
                    applications = applicationsList.OrderBy(a => a.TermMonths);
                    break;
                case "TermMonthsDesc":
                    applications = applicationsList.OrderByDescending(a => a.TermMonths);
                    break;
                case "RequestedAmountAsc":
                    applications = applicationsList.OrderBy(a => a.RequestedAmount);
                    break;
                case "RequestedAmountDesc":
                    applications = applicationsList.OrderByDescending(a => a.RequestedAmount);
                    break;
                case "ApprovedAmountAsc":
                    applications = applicationsList.OrderBy(a => a.ApprovedAmount);
                    break;
                case "ApprovedAmountDesc":
                    applications = applicationsList.OrderByDescending(a => a.ApprovedAmount);
                    break;
                case "PurposeAsc":
                    applications = applicationsList.OrderBy(a => a.Purpose);
                    break;
                case "PurposeDesc":
                    applications = applicationsList.OrderByDescending(a => a.Purpose);
                    break;
                case "ApplicationDateAsc":
                    applications = applicationsList.OrderBy(a => a.ApplicationDate);
                    break;
                case "ApplicationDateDesc":
                    applications = applicationsList.OrderByDescending(a => a.ApplicationDate);
                    break;
                case "DecisionDateAsc":
                    applications = applicationsList.OrderBy(a => a.DecisionDate);
                    break;
                case "DecisionDateDesc":
                    applications = applicationsList.OrderByDescending(a => a.DecisionDate);
                    break;
                default:
                    applications = applicationsList.OrderBy(a => a.ApplicationId);
                    break;
            }


            ViewBag.Status = Status;
            ViewBag.CustomerId = customerId;
            ViewBag.Applications = applications;
            int PageSize = 10; 
            int PageNumber = page ?? 1; 

            var applicationsPaged = applications.ToPagedList(PageNumber, PageSize);
            return View(applicationsPaged);
        }



        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetById(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }
            return View(application);
        }

        // GET: LoanApplication/CustomerGetById - For customers to view their own application details
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> CustomerGetById(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
            }

            // Verify the customer owns this application using the injected service
            var currentCustomer = await _customerService.GetByEmailAsync(User.Identity.Name);
            if (currentCustomer == null || currentCustomer.CustomerId != application.CustomerId)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View("CustomerGetById", application);
        }

        [HttpGet]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> Create(int? customerId, int? productId)
        {
            // If user is Customer, they can only create applications for themselves
            if (User.IsInRole("Customer"))
            {
                if (customerId.HasValue)
                {
                    // Verify the customer is creating for themselves
                    var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                    if (customerService != null)
                    {
                        var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                        if (currentCustomer == null || currentCustomer.CustomerId != customerId.Value)
                        {
                            return RedirectToAction("AccessDenied", "Home");
                        }
                    }
                }
            }
            // Admin and SuperAdmin can create applications for any customer
             PopulateLoanApplicationPurpose();
            await PopulateLoanProductsDropdown();
            ViewBag.CustomerId = customerId;
            ViewBag.ProductId = productId;

            var loanApplicationDto = new LoanApplicationDto
            {
                CustomerId = customerId ?? 0,
                ProductId = productId ?? 0
            };


            return View(loanApplicationDto);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> Create(LoanApplicationDto loanApplicationDto)
        {
            // If user is Customer, they can only create applications for themselves
            if (User.IsInRole("Customer"))
            {
                // Verify the customer is creating for themselves
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != loanApplicationDto.CustomerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }
            // Admin and SuperAdmin can create applications for any customer

            if (!ModelState.IsValid)
            {
                await PopulateLoanProductsDropdown();
                return View(loanApplicationDto);
            }

            try
            {
                loanApplicationDto.CreatedBy =Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _loanApplicationService.CreateAsync(loanApplicationDto);
                if (result)
                {
                    TempData["LoanApplicationSuccess"] = "Loan application submitted successfully!";
                    return RedirectToAction("CustomerView", new { customerId = loanApplicationDto.CustomerId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create loan application.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateLoanProductsDropdown();
            return View(loanApplicationDto);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]

        public async Task<IActionResult> Approve(int id)
        {

            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("~/Views/Shared/NotFound.cshtml");
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
                    ["Purpose"] = EnumHelper.GetDescription((LoanApplicationPurpose)loanApplicationDto.Purpose)
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
                return View("~/Views/Shared/NotFound.cshtml");
            }
            await PopulateLoanProductsDropdown();

            return View(application);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateModel]


        public async Task<ActionResult> RejectAsync(int applicationId)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("Index", "Home");
            }

            var loanApplication = await _loanApplicationService.GetByIdAsync(applicationId);
            if (loanApplication == null)
            {
                TempData["Error"] = "Loan application not found.";
                return RedirectToAction("Index");
            }

            if (loanApplication.CreatedBy == userId)
            {
                TempData["Error"] = "You cannot reject your own application.";
                return RedirectToAction("GetById", new { id = applicationId });
            }

            loanApplication.RejectedBy = userId;
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
                            ["Purpose"] = EnumHelper.GetDescription((LoanApplicationPurpose)application.Purpose)
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
        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        [HttpGet("/LoanApplication/CustomerView/{customerId}")]
        public async Task<IActionResult> CustomerView(int customerId)
        {
            // If user is Customer, verify they are viewing their own applications
            if (User.IsInRole("Customer"))
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != customerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }
            // Admin and SuperAdmin can view any customer's applications

            var applications = await _loanApplicationService.GetByCustomerIdAsync(customerId);
            var applicationsList = applications.ToList();

            ViewBag.CustomerId = customerId;
            ViewBag.Applications = applicationsList;

            return View(applicationsList);
        }

        // GET: LoanApplication/MyApplications - For customers to view their own applications
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            // Get the current customer using the injected service
            var currentCustomer = await _customerService.GetByEmailAsync(User.Identity.Name);
            if (currentCustomer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index", "Home");
            }

            // Get the customer's applications
            var applications = await _loanApplicationService.GetByCustomerIdAsync(currentCustomer.CustomerId);
            var applicationsList = applications.ToList();

            ViewBag.CustomerId = currentCustomer.CustomerId;
            ViewBag.Applications = applicationsList;

            return View("CustomerView", applicationsList);
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



        [HttpGet]
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<FileResult> Export(string status = null)
        {
            
                var applications = await _loanApplicationService.GetAllAsync();

            var FilteredApplications = applications.Where(a => string.IsNullOrEmpty(status) || a.Status.ToString() == status).ToList();

            var dt = new DataTable("LoanApplications");
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Application ID"),
                    new DataColumn("Customer Name"),
                    new DataColumn("Product Name"),
                    new DataColumn("Customer ID"),
                    new DataColumn("Product ID"),
                    new DataColumn("Status"),
                    new DataColumn("Term (Months)"),
                    new DataColumn("Requested Amount"),
                    new DataColumn("Approved Amount"),
                    new DataColumn("Purpose"),
                    new DataColumn("Application Date"),
                    new DataColumn("Decision Date")
                });

                foreach (var app in FilteredApplications)
                {
                    dt.Rows.Add(
                        app.ApplicationId,
                        $"{app.FirstName} {app.LastName}",
                        app.ProductName,
                        app.CustomerId,
                        app.ProductId,
                        app.Status,
                        app.TermMonths,
                        app.RequestedAmount.ToString("C"),
                        app.ApprovedAmount.ToString("C") ,
                        app.Purpose,
                        app.ApplicationDate.ToOffset(TimeSpan.FromHours(3)).ToString("dd/MM/yyyy"),
                        app.DecisionDate != default(DateTimeOffset) ? app.DecisionDate.ToOffset(TimeSpan.FromHours(3)).ToString("dd/MM/yyyy") : "N/A"
                    );
                }

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Loan Applications");
                    ws.Cell(1, 1).InsertTable(dt);
                    ws.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "LoanApplications.xlsx"
                        );
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

            // Log Audit for Disbursement (only when account is created)
            var oldStatus = EnumHelper.GetDescription(LoanStatus.Approved);
            var actionText = $"The loan application of amount {application.ApprovedAmount} has been disbursed";
            await _auditService.AddLoanDisbursementAuditAsync(
                application.ApplicationId,
                createdAccount.AccountId,
                actionText,
                oldStatus,
                "Disbursed"
            );

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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetApplicationsByCustomerId(int customerId)
        {
            // If user is Customer, verify they are viewing their own applications
            if (User.IsInRole("Customer"))
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var currentCustomer = await customerService.GetByEmailAsync(User.Identity.Name);
                    if (currentCustomer == null || currentCustomer.CustomerId != customerId)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
            }
            // Admin and SuperAdmin can view any customer's applications
            var applications = await _loanApplicationService.GetByCustomerIdAsync(customerId);
            return View(applications);
        }

        private void PopulateLoanApplicationPurpose()
        {
            ViewBag.LoanPurpose = Enum.GetValues(typeof(LoanApplicationPurpose))
                    .Cast<LoanApplicationPurpose>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString().Replace("_", " ")
                    }).ToList();
        }


    }
}

 

