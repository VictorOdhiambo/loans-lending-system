using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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

        public LoanApplicationController(
            ILoanApplicationService loanApplicationService,
            ILoanProductService loanProductService,
            INotificationSenderService notificationSenderService,
            ICustomerService customerService)
        {
            _loanApplicationService = loanApplicationService;
            _loanProductService = loanProductService;
            _notificationSenderService = notificationSenderService;
            _customerService = customerService;
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
                            ["LoanAmount"] = application.ApprovedAmount?.ToString("F2") ?? application.RequestedAmount.ToString("F2"),
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
            var LoanProducts = await _loanProductService.GetAllProducts();
            var loanProductSelectList = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%|{lp.MinTermMonths}|{lp.MaxTermMonths}|{lp.MinAmount}|{lp.MaxAmount}|{lp.InterestRate}",
                Selected = productId.HasValue && lp.ProductId == productId.Value
            }).ToList();

            ViewBag.LoanProducts = loanProductSelectList;

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

            var dto = new LoanApplicationDto();
            dto.CustomerId = actualCustomerId;
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
                if (loanApplicationDto.CustomerId != 0)
                {
                    var customer = await _customerService.GetByIdAsync(loanApplicationDto.CustomerId);
                    if (customer != null)
                    {
                        ViewBag.CustomerName = customer.FirstName + " " + customer.LastName;
                        ViewBag.CustomerId = customer.CustomerId;
                    }
                }
                return View(loanApplicationDto);
            }

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
            //get loan products name and interest rate for dropdown
            var LoanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            return View(application);
            
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<IActionResult> Approve(LoanApplicationDto dto)
        {
            if (!dto.ApprovedAmount.HasValue)
            {
                ModelState.AddModelError(nameof(dto.ApprovedAmount), "Approved Amount is required.");
            }

            if (ModelState.IsValid)
            {
                await _loanApplicationService.ApproveAsync(dto.ApplicationId, dto.ApprovedAmount.Value);
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
                TempData["LoanApplicationSuccess"] = "Loan application approved successfully!";
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


        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
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

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> RejectAsync(int applicationId)
        {
            if (ModelState.IsValid)
            {
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
                    TempData["LoanApplicationSuccess"] = "Loan application rejected successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Unexpected error occurred while rejecting the application.";
            }
            TempData["Error"] = "Loan application rejection failed. Please check validation errors.";
            return RedirectToAction("GetById", new { id = applicationId });
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

        
    }
}
