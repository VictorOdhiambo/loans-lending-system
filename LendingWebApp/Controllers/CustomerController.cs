using LoanApplicationService.Service.DTOs.CustomerModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using LoanApplicationService.Core.Models;
using BCrypt.Net;
using LoanApplicationService.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
private readonly LoanApplicationService.Web.Helpers.IEmailService _emailService;
private readonly INotificationSenderService _notificationSenderService;
private readonly LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext _context;

public CustomerController(ICustomerService customerService, LoanApplicationService.Web.Helpers.IEmailService emailService, INotificationSenderService notificationSenderService, LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext context)
{
    _customerService = customerService;
    _emailService = emailService;
    _notificationSenderService = notificationSenderService;
    _context = context;
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
    // Fetch customer info
    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
    var data = new Dictionary<string, string>
    {
        ["Email"] = dto.Email,
        ["FirstName"] = dto.FirstName,
        ["LastName"] = dto.LastName,
        ["FullName"] = dto.FullName,
        ["PhoneNumber"] = dto.PhoneNumber,
        ["Address"] = dto.Address ?? string.Empty,
        ["DateOfBirth"] = dto.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
        ["NationalId"] = dto.NationalId ?? string.Empty,
        ["EmploymentStatus"] = dto.EmploymentStatus ?? string.Empty,
        ["AnnualIncome"] = dto.AnnualIncome?.ToString() ?? string.Empty
    };
    // Fetch most recent loan application for this customer
    if (customer != null)
    {
        var loanApp = await _context.LoanApplications
            .Where(l => l.CustomerId == customer.CustomerId)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();
        if (loanApp != null)
        {
            data["LoanAmount"] = loanApp.RequestedAmount.ToString("F2");
            data["LoanProductId"] = loanApp.ProductId.ToString();
        }
    }
    await _notificationSenderService.SendNotificationAsync(
        "Account Created", // match the template header in the DB
        "email",
        data
    );
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
