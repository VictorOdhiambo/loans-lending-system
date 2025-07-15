using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Threading.Tasks;


namespace LoanApplicationService.Web.Controllers
{
    public class LoanApplicationController(ILoanApplicationService loanApplicationService, ILoanProductService loanProductService) : Controller
    {
        private readonly ILoanApplicationService _loanApplicationService = loanApplicationService;
        private readonly ILoanProductService _loanProductService = loanProductService;

        [HttpGet]
        public async Task<IActionResult> Index(int? Status)
        {
            var applications = await _loanApplicationService.GetAllAsync();

            if (Status.HasValue)
            {
                applications = applications.Where(a => a.Status == (LoanStatus)Status.Value);
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
        public async Task<IActionResult> Create()
        {
            var LoanProducts = await _loanProductService.GetAllProducts();
            //get loan products name and interest rate for dropdown
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();


            return View(new LoanApplicationDto());
        }




        [HttpPost]
        public async Task<IActionResult> Create(LoanApplicationDto loanApplicationDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _loanApplicationService.CreateAsync(loanApplicationDto);
                if (result)
                {
                    TempData["Success"] = "Loan application created successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Unexpected error occurred while saving.";
            }
            TempData["Error"] = "Loan application creation failed. Please check validation errors.";
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
            if (ModelState.IsValid)
            {
                await _loanApplicationService.ApproveAsync(dto.ApplicationId);
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
