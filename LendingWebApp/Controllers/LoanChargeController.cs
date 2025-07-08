using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationService.Web.Controllers
{
    public class LoanChargeController(ILoanChargeService loanChargeService) : Controller
    {
        private readonly ILoanChargeService _loanChargeService = loanChargeService;



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var charges = await _loanChargeService.GetAllCharges();
            return View(charges.ToList());
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            return View(new LoanChargeDto());
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
        public async Task<ActionResult> Update( int id)
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
    }

}
