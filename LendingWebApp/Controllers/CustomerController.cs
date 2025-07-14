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

            // Check if user already exists
            var db = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
            if (db.Users.Any(u => u.Email == dto.Email))
            {
                TempData["Error"] = "A user with this email already exists.";
                return View(dto);
            }

            // Create user
            var user = new Users
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Username = dto.FirstName + " " + dto.LastName,
                Role = "Customer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsDeleted = false
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Create customer and link to user
            var customer = new LoanApplicationService.Core.Models.Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                NationalId = dto.NationalId,
                EmploymentStatus = dto.EmploymentStatus,
                AnnualIncome = dto.AnnualIncome,
                UserId = user.Id
            };

            var result = await _customerService.CreateAsync(customer);
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
