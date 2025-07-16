using LoanApplicationService.Service.DTOs.CustomerModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using LoanApplicationService.Core.Models;
using BCrypt.Net;

namespace LoanApplicationService.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View(new CustomerDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Customer creation failed. Please check validation errors.";
                return View(dto);
            }

            if (await _customerService.UserExistsAsync(dto.Email))
            {
                TempData["Error"] = "A user with this email already exists.";
                return View(dto);
            }

            var result = await _customerService.CreateUserAndCustomerAsync(dto);
            if (result)
            {
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Unexpected error occurred while saving.";
            return View(dto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _customerService.UpdateAsync(dto);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }
    }
}
