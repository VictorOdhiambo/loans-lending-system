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


namespace LoanApplicationService.Web.Controllers
{
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
                TempData["Success"] = "Loan application disbursed successfully!";
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

        
    }
}
