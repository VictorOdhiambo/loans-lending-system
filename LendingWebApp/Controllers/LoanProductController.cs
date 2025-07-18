using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace LoanApplicationService.Web.Controllers
{
    public class LoanProductController(ILoanProductService loanProductService) : Controller
    {
        private readonly ILoanProductService _loanProductService = loanProductService;


        [HttpGet]
        public async Task<IActionResult> Index(int? customerId)
        {
            ViewBag.CustomerId = customerId;
            LoanApplicationService.Service.DTOs.CustomerModule.CustomerDto customer = null;
            if (customerId.HasValue)
            {
                // Fetch customer details for display
                var customerService = HttpContext.RequestServices.GetService(typeof(ICustomerService)) as ICustomerService;
                if (customerService != null)
                {
                    customer = await customerService.GetByIdAsync(customerId.Value);
                    if (customer != null)
                    {
                        ViewBag.CustomerName = customer.FirstName + " " + customer.LastName;
                    }
                }
            }
            var loanProducts = await _loanProductService.GetAllProducts();
            if (customer != null)
            {
                List<LoanProductDto> filtered;
                var risk = customer.RiskLevel;
                if (risk == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryLow)
                {
                    filtered = loanProducts.ToList();
                }
                else if (risk == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.Low)
                {
                    filtered = loanProducts.Where(lp => lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.Low ||
                                                        lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.Medium ||
                                                        lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.High ||
                                                        lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh).ToList();
                }
                else if (risk == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.Medium)
                {
                    filtered = loanProducts.Where(lp => lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.Medium ||
                                                        lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.High ||
                                                        lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh).ToList();
                }
                else if (risk == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.High)
                {
                    filtered = loanProducts.Where(lp => lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.High ||
                                                        lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh).ToList();
                }
                else // VeryHigh
                {
                    filtered = loanProducts.Where(lp => lp.RiskLevel == LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh).ToList();
                }
                if (!filtered.Any())
                {
                    ViewBag.NoProductsMessage = "No loan products match this customer's risk level.";
                }
                return View(filtered);
            }
            return View(loanProducts);
        }


        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var loanProducts = await _loanProductService.GetLoanProductById(id);
            return View(loanProducts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new LoanProductDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(LoanProductDto loanProductDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _loanProductService.AddLoanProduct(loanProductDto);
                if (result)
                {
                    TempData["Success"] = "Loan product created successfully!";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Unexpected error occurred while saving.";
            }

            TempData["Error"] = "Loan product creation failed. Please check validation errors.";
            PopulateDropdowns();
            return View(loanProductDto);
        }





        [HttpGet]
        public async Task<ActionResult> Modify(int id)
        {
            var LoanProduct = await _loanProductService.GetLoanProductById(id);
            PopulateDropdowns();
            return View(LoanProduct);
        }


        [HttpPost]
        public async Task<ActionResult> Modify(LoanProductDto loanProductDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _loanProductService.ModifyLoanProduct(loanProductDto.ProductId, loanProductDto);
                if (result)
                {
                    TempData["Success"] = "Loan product updated successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Failed to update loan product. Please try again.";
            }
            else
            {
                TempData["Error"] = "Validation failed. Please check the form for errors.";
            }
            PopulateDropdowns();
            return View(loanProductDto);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid loan product ID.";
                return RedirectToAction("Index");
            }
            var result = await _loanProductService.DeleteLoanProduct(id);
            if (result)
            {
                TempData["Success"] = "Loan product deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete loan product. Please try again.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public  async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid loan product ID.";
                return RedirectToAction("Index");
            }
            var loanProduct = await _loanProductService.GetLoanProductById(id);
            if (loanProduct == null)
            {
                TempData["Error"] = "Loan product not found.";
                return RedirectToAction("Index");
            }
            return View(loanProduct);
        }



        private void PopulateDropdowns()
        {
            ViewBag.LoanProductTypes = Enum.GetValues(typeof(LoanProductType))
                .Cast<LoanProductType>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();

            ViewBag.PaymentFrequencies = Enum.GetValues(typeof(PaymentFrequency))
                .Cast<PaymentFrequency>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();

            ViewBag.RiskLevels = Enum.GetValues(typeof(LoanRiskLevel))
                .Cast<LoanRiskLevel>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = EnumHelper.GetDescription(e)
                }).ToList();
        }

    }
}


