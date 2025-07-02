using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Web.Controllers
{
    [Route("/loan-products")]
    public class LoanProductController(ILoanProductService loanProductService) : Controller
    {
        private readonly ILoanProductService _loanProductService = loanProductService;


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var loanProducts = await _loanProductService.GetAllProducts();
            return View(loanProducts);
        }


        [HttpGet("/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var loanProducts = await _loanProductService.GetLoanProductById(id);
            return View(loanProducts);
        }

        [HttpGet("/create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet("/modify")]
        public IActionResult Modify()
        {
            return View();
        }

        [HttpPost("/create")]
        public async Task<IActionResult> Create(LoanProductDto loanProductDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Loan product creation failed. All fields are required!";

                return RedirectToAction("Create");
            }

            var isSuccess = await _loanProductService.AddLoanProduct(loanProductDto);
            if (isSuccess)
            {
                TempData["Success"] = "Loan product created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Loan product creation failed. Please try again.";
                return RedirectToAction("Create");
            }

        }

        //update loan product
        [HttpPut("/modify/{id}")]
        public async Task<IActionResult> Modify(int id, LoanProductDto loanProductDto)
        {

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Loan product modification failed. All fields are required!";
                return RedirectToAction("Modify");
            }

            var isSuccess = await _loanProductService.ModifyLoanProduct(id, loanProductDto);
            if (isSuccess)
            {
                TempData["Success"] = "Loan product modified successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Loan product modify failed. Please try again.";
                return RedirectToAction("Modify");
            }

        }

    }

}