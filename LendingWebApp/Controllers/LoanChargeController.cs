using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;

namespace LoanApplicationService.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class LoanChargeController(ILoanChargeService loanChargeService, ILoanProductService loanProductService) : Controller
    {
        private readonly ILoanChargeService _loanChargeService = loanChargeService;
        private readonly ILoanProductService _loanProductService = loanProductService;



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var charges = await _loanChargeService.GetAllCharges();
            return View(charges.ToList());
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            return View(new LoanChargeDto { Name = "", Description = "" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(LoanChargeDto loanChargeDto)
        {
            if (ModelState.IsValid)
            {
                await _loanChargeService.AddLoanCharge(loanChargeDto);
                return RedirectToAction("Index");
            }
            return View(loanChargeDto);
        }

        [HttpGet]
        public async Task<ActionResult> Update(int id)
        {
            var dto = await _loanChargeService.GetLoanChargeById(id);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(LoanChargeDto loanChargeDto)
        {
            if (ModelState.IsValid)
            {
                await _loanChargeService.UpdateLoanCharge(loanChargeDto);

                return RedirectToAction("Index");
            }
            return View(loanChargeDto);
        }

        [HttpGet]

        public async Task<ActionResult> Delete(int id)
        {
            var loanCharge = await _loanChargeService.GetLoanChargeById(id);
            if (loanCharge == null)
            {
                return NotFound();
            }
            return View(loanCharge);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loanCharge = await _loanChargeService.GetLoanChargeById(id);
            if (loanCharge == null)
            {
                return NotFound();
            }
            await _loanChargeService.DeleteLoanCharge(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        //get charges for a loan product
        public async Task<IActionResult> GetChargesForLoanProduct(int loanProductId)
        {
            var charges = await _loanChargeService.GetAllChargesForLoanProduct(loanProductId);
            return View(charges.ToList());

        }

        [HttpGet]
        public async Task<IActionResult> AddChargeToLoanProduct()
        {
            var LoanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            var LoanCharges = await _loanChargeService.GetAllCharges();
            ViewBag.LoanCharges = LoanCharges.Select(lc => new SelectListItem
            {
                Value = lc.LoanChargeId.ToString(),
                Text = $"{lc.Name} - {lc.Amount} {lc.Description}"
            }).ToList();
            return View();
        }               

        [HttpPost]
        public async Task<IActionResult> AddChargeToLoanProduct(LoanChargeMapperDto LoanChargeMap)
        {

            var LoanProducts = await _loanProductService.GetAllProducts();
            ViewBag.LoanProducts = LoanProducts.Select(lp => new SelectListItem
            {
                Value = lp.ProductId.ToString(),
                Text = $"{lp.ProductName} - {lp.InterestRate}%"
            }).ToList();
            var LoanCharges = await _loanChargeService.GetAllCharges();
            ViewBag.LoanCharges = LoanCharges.Select(lc => new SelectListItem
            {
                Value = lc.LoanChargeId.ToString(),
                Text = $"{lc.Name} - {lc.Amount} {lc.Description}"
            }).ToList();

            if (ModelState.IsValid)
            {
                var result = await _loanChargeService.AddChargeToProduct(LoanChargeMap);
                if (result)
                {
                    TempData["Success"] = "Charge added to product successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Unexpected error occurred while adding charge to product.";
            }
            TempData["Error"] = "Failed to add charge to product. Please check validation errors.";
            return RedirectToAction("Index");
        }
    }
}