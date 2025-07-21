using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace LoanApplicationService.Web.Controllers
{
    public class LoanApplicationController : Controller
    {
        private readonly ILoanApplicationService _loanApplicationService;
        private readonly ILoanProductService _loanProductService;
        private readonly INotificationSenderService _notificationSenderService;
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;

        public LoanApplicationController(
            ILoanApplicationService loanApplicationService,
            ILoanProductService loanProductService,
            INotificationSenderService notificationSenderService,
            ICustomerService customerService,
            IAccountService accountService
            )

        {
            _loanApplicationService = loanApplicationService;
            _loanProductService = loanProductService;
            _notificationSenderService = notificationSenderService;
            _customerService = customerService;
            _accountService = accountService;
        }

        

        [HttpGet]
        public async Task<IActionResult> Index(int? Status, int? customerId)
        {
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
            var LoanProducts = await _loanProductService.GetAllProducts();
            var loanProductSelectList = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%|{lp.MinTermMonths}|{lp.MaxTermMonths}|{lp.MinAmount}|{lp.MaxAmount}|{lp.InterestRate}",
                Selected = productId.HasValue && lp.ProductId == productId.Value
            }).ToList();

            ViewBag.LoanProducts = loanProductSelectList;

            string customerName = null;
            if (customerId.HasValue)
            {
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    var customer = await customerService.GetByIdAsync(customerId.Value);
                    if (customer != null)
                    {
                        customerName = customer.FirstName + " " + customer.LastName;
                        ViewBag.CustomerName = customerName;
                    }
                }
            }

            var dto = new LoanApplicationDto();
            if (customerId.HasValue) dto.CustomerId = customerId.Value;
            if (productId.HasValue) dto.ProductId = productId.Value;

            return View(dto);
        }




        [HttpPost]
        public async Task<IActionResult> Create(LoanApplicationDto loanApplicationDto)
        {
            // Fetch the selected loan product
            var product = await _loanProductService.GetLoanProductById(loanApplicationDto.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Invalid loan product selected.");
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

            if (!ModelState.IsValid)
            {
                // Re-populate ViewBag.LoanProducts for the view
                var LoanProducts = await _loanProductService.GetAllProducts();
                var loanProductSelectList = LoanProducts.Select(lp => new SelectListItem
                {
                    Value = lp.ProductId.ToString(),
                    Text = $"{lp.ProductName} - {lp.InterestRate}%|{lp.MinTermMonths}|{lp.MaxTermMonths}|{lp.MinAmount}|{lp.MaxAmount}|{lp.InterestRate}",
                    Selected = lp.ProductId == loanApplicationDto.ProductId
                }).ToList();
                ViewBag.LoanProducts = loanProductSelectList;
                return View(loanApplicationDto);
            }

            //get user or customer Id
            var userIdStr =  HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("Login", "Home");
            }
            loanApplicationDto.CreatedBy = Guid.Parse(userIdStr);

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
                TempData["Success"] = "Loan application created successfully!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Unexpected error occurred while saving.";
            return View(loanApplicationDto);
        }


        [HttpGet]
        [RoleAuthorize("Admin")]

        public async Task<IActionResult> Approve(int id)
        {
            
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("NotFound");
            }
            //get loan products name and interest rate for dropdown
            var LoanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            return View(application);
            
        }


        [HttpPost]
        [RoleAuthorize("Admin")]
        public async Task<IActionResult> Approve(LoanApplicationDto dto)
        {
            if (!dto.ApprovedAmount.HasValue)
            {
                ModelState.AddModelError(nameof(dto.ApprovedAmount), "Approved Amount is required.");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("", "Home");
            }
            var LoanApplication = await _loanApplicationService.GetByIdAsync(dto.ApplicationId);
            if (LoanApplication == null)
            {
                TempData["Error"] = "Loan application not found.";
                return RedirectToAction("Index");
            }

            var ApprovedBy = Guid.Parse((HttpContext.Session.GetString("UserId")));
            if ( LoanApplication.CreatedBy == ApprovedBy) { 
                ModelState.AddModelError(nameof(dto.ApprovedBy), "You cannot approve your own application.");
                TempData["Error"] = "You cannot approve your own application.";
                return RedirectToAction("GetById", new { id = dto.ApplicationId });
            }
            if (ModelState.IsValid)
            {
                await _loanApplicationService.ApproveAsync(dto.ApplicationId, dto.ApprovedAmount.Value, ApprovedBy);
                // Send notification to customer using the custom template
                var customer = await _customerService.GetByIdAsync(dto.CustomerId);
                if (customer != null)
                {
                    var data = new Dictionary<string, string>
                    {
                        ["FirstName"] = customer.FirstName,
                        ["LastName"] = customer.LastName,
                        ["FullName"] = customer.FirstName + " " + customer.LastName,
                        ["Email"] = customer.Email,
                        ["LoanProductId"] = dto.ProductId.ToString(),
                        ["LoanAmount"] = dto.ApprovedAmount?.ToString("F2") ?? dto.RequestedAmount.ToString("F2"),
                        ["TermMonths"] = dto.TermMonths.ToString(),
                        ["Purpose"] = dto.Purpose ?? string.Empty
                    };
                    await _notificationSenderService.SendNotificationAsync(
                        "Loan Approved",
                        "email",
                        data
                    );
                }
                TempData["Success"] = "Loan application approved successfully!";
                return RedirectToAction("Index");
            }
            var LoanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            return View(dto);
        }


        [HttpGet]
        [RoleAuthorize("Admin")]

        public async Task<IActionResult> Reject(int id)
        {
            var application = await _loanApplicationService.GetByIdAsync(id);
            if (application == null)
            {
                return View("NotFound");
            }
            //get loan products name and interest rate for dropdown
            var LoanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            return View(application);
        }

        [HttpPost]
        [RoleAuthorize("Admin")]


        public async Task<ActionResult> RejectAsync(int applicationId)
        {
            if (ModelState.IsValid)
            {
                var userIdStr =Guid.Parse (HttpContext.Session.GetString("UserId"));
                var loanApplication = await _loanApplicationService.GetByIdAsync(applicationId);
                if (loanApplication.CreatedBy == userIdStr)
                
                {
                    TempData["Error"] = "You cannot reject your own application.";
                    return RedirectToAction("GetById", new { id = applicationId });
                }

                loanApplication.RejectedBy = userIdStr;
                var result = await _loanApplicationService.RejectAsync(applicationId);
                if (result != null)
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
                    TempData["Success"] = "Loan application rejected successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Unexpected error occurred while rejecting the application.";
            }
            TempData["Error"] = "Loan application rejection failed. Please check validation errors.";
            return RedirectToAction("GetById", new { id = applicationId });
        }

        //customer views
        // GET: LoanApplication/CustomerView
        [HttpGet("/LoanApplication/CustomerView/{customerId}")]
        public async Task<IActionResult> CustomerView(int customerId)
        {
            var applications = await _loanApplicationService.GetByCustomerIdAsync(customerId);
            return View(applications);
        }

        // POST: LoanApplication/CustomerReject

        [HttpPost]
        public async Task<IActionResult> CustomerReject(int applicationId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Reason for rejection is required.";
                return RedirectToAction("CustomerView", new { customerId = applicationId });
            }
            var result = await _loanApplicationService.CustomerReject(applicationId, reason);
            if (result)
            {
                TempData["Success"] = "Loan application rejected successfully!";
            }
            else
            {
                TempData["Error"] = "Unexpected error occurred while rejecting the application.";
            }
            return RedirectToAction("CustomerView", new { customerId = applicationId });
        }

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



        [HttpPost]
        public async Task<IActionResult> Disburse(int applicationId)
        {
            var application = await _loanApplicationService.GetByIdAsync(applicationId);
            var accountDto = new AccountDto
            {
                ApplicationId = application.ApplicationId,
                CustomerId = application.CustomerId,
                AccountNumber = "ACCT" + application.ApplicationId,
                AccountType = "Loan",
                PrincipalAmount = application.ApprovedAmount ?? 0,
                DisbursedAmount = application.ApprovedAmount ?? 0,
                AvailableBalance = application.ApprovedAmount ?? 0,
                OutstandingBalance = application.ApprovedAmount ?? 0,
                InterestRate = application.InterestRate ?? 0,
                TermMonths = application.TermMonths,
                MonthlyPayment = (application.ApprovedAmount ?? 0) / application.TermMonths,
                NextPaymentDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                DisbursementDate = DateTime.UtcNow,
                MaturityDate = DateTime.UtcNow.AddMonths(application.TermMonths)
            };

            await _accountService.CreateAccountAsync(accountDto);

            application.Status = LoanStatus.Disbursed;
            await _loanApplicationService.UpdateAsync(application);

            return RedirectToAction("Index");



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
    }

}

