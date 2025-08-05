using LoanApplicationService.Service.DTOs.CustomerModule;
using LoanApplicationService.Service.DTOs.Account;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using LoanApplicationService.Core.Models;
using BCrypt.Net;
using LoanApplicationService.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LoanApplicationService.Web.Services;

namespace LoanApplicationService.Web.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly EmailService _emailService;
        private readonly INotificationSenderService _notificationSenderService;
        private readonly LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(
            ICustomerService customerService,
            EmailService emailService,
            INotificationSenderService notificationSenderService,
            LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _customerService = customerService;
            _emailService = emailService;
            _notificationSenderService = notificationSenderService;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();
            return View(customers);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult Create()
        {
            return View(new CustomerDto());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["SelfRegister"] = true;
            return View("Create");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CustomerDto dto)
        {
            ViewData["SelfRegister"] = true;
            if (!ModelState.IsValid)
                return View("Create", dto);
            return await Create(dto);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Customer creation failed. Please check validation errors.";
                if (ViewData["SelfRegister"] as bool? ?? false)
                    return View("Create", dto);
                return View(dto);
            }

            // Check for existing email or NationalId
            if (await _customerService.EmailOrNationalIdExistsAsync(dto.Email, dto.NationalId))
            {
                TempData["Error"] = "A customer with this email or national ID already exists.";
                if (ViewData["SelfRegister"] as bool? ?? false)
                    return View("Create", dto);
                return View(dto);
            }

            // Create user using UserManager to ensure proper Identity setup
            var role = await _context.ApplicationRoles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (role == null)
            {
                TempData["Error"] = "Customer role not found.";
                if (ViewData["SelfRegister"] as bool? ?? false)
                    return View("Create", dto);
                return View(dto);
            }

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email, 
                IsActive = true
            };

            var userResult = await _userManager.CreateAsync(user, dto.Password ?? string.Empty);
            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                if (ViewData["SelfRegister"] as bool? ?? false)
                    return View("Create", dto);
                return View(dto);
            }

            // Add user to Customer role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Create customer record
            var customer = new Customer
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
                UserId = user.Id,
                User = user
            };

            // Set RiskLevel
            int age = 0;
            if (dto.DateOfBirth.HasValue)
            {
                var today = DateTime.UtcNow;
                age = today.Year - dto.DateOfBirth.Value.Year;
                if (dto.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            }
            decimal income = dto.AnnualIncome ?? 0;
            if (age > 0 && dto.EmploymentStatus != null && dto.AnnualIncome.HasValue)
                customer.RiskLevel = LoanApplicationService.CrossCutting.Utils.RiskScoringUtil.GetRiskLevel(age, dto.EmploymentStatus, income);
            else
                customer.RiskLevel = LoanApplicationService.CrossCutting.Utils.LoanRiskLevel.VeryHigh;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Add CustomerId to the data dictionary
            var data = new Dictionary<string, string>
            {
                ["CustomerId"] = customer.CustomerId.ToString(), 
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
                "Account Created", 
                "email",
                data
            );
            
            
            if (ViewData["SelfRegister"] as bool? ?? false)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                return RedirectToAction("CustomerDashboard", "Home");
            }

            // Send notification for admin-created customers
            var adminData = new Dictionary<string, string>
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
                    adminData["LoanAmount"] = loanApp.RequestedAmount.ToString("F2");
                    adminData["LoanProductId"] = loanApp.ProductId.ToString();
                }
            }
            
            await _notificationSenderService.SendNotificationAsync(
                "Account Created", 
                "email",
                adminData
            );
            
            TempData["CustomerSuccess"] = "Customer created successfully!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerDto dto)
        {
            Console.WriteLine($"[Edit POST] DTO: {dto.FirstName}, {dto.LastName}, {dto.DateOfBirth}");
            if (!ModelState.IsValid)
                return View(dto);

            var updated = await _customerService.UpdateAsync(dto.CustomerId, dto);
            if (updated == null)
            {
                TempData["Error"] = "Customer not found or failed to update.";
                return View(dto);
            }
            TempData["CustomerSuccess"] = "Customer updated successfully!";
            return RedirectToAction("Details", new { id = dto.CustomerId });
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        { 
            var deleted = await _customerService.DeleteAsync(id);
            if (!deleted)
            {
                TempData["Error"] = "Failed to delete customer.";
                return RedirectToAction("Index");
            }
            TempData["CustomerSuccess"] = "Customer deleted successfully!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            Console.WriteLine($"[Details GET] Customer: {customer?.FirstName}, {customer?.LastName}, {customer?.DateOfBirth}");
            if (customer == null) return NotFound();

            return View(customer);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            var passwordDto = new AdminChangeCustomerPasswordDto
            {
                CustomerId = customer.CustomerId
            };

            ViewBag.CustomerName = customer.FullName;
            return View(passwordDto);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(AdminChangeCustomerPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var customerForViewBag = await _customerService.GetByIdAsync(dto.CustomerId);
                ViewBag.CustomerName = customerForViewBag?.FullName;
                return View(dto);
            }

            // Get customer details
            var customer = await _customerService.GetByIdAsync(dto.CustomerId);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index");
            }

            // Find the user account for this customer
            var user = await _userManager.FindByEmailAsync(customer.Email);
            if (user == null)
            {
                TempData["Error"] = "User account not found for this customer.";
                return RedirectToAction("Details", new { id = dto.CustomerId });
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to update password. Please try again.";
                return RedirectToAction("Details", new { id = dto.CustomerId });
            }

            TempData["CustomerSuccess"] = $"Password for {customer.FullName} has been updated successfully!";
            return RedirectToAction("Details", new { id = dto.CustomerId });
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity.Name;
            var customers = await _customerService.GetAllAsync();
            var currentCustomer = customers.FirstOrDefault(c => c.Email == email);
            if (currentCustomer == null)
                return NotFound();
            ViewData["IsProfile"] = true;
            ViewData["IsCustomerProfile"] = true; 
            return View("Details", currentCustomer);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var email = User.Identity.Name;
            var customers = await _customerService.GetAllAsync();
            var currentCustomer = customers.FirstOrDefault(c => c.Email == email);
            if (currentCustomer == null)
                return NotFound();
            return View(currentCustomer);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(CustomerDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var email = User.Identity.Name;
            var customers = await _customerService.GetAllAsync();
            var currentCustomer = customers.FirstOrDefault(c => c.Email == email);
            if (currentCustomer == null || currentCustomer.CustomerId != dto.CustomerId)
            {
                TempData["Error"] = "You can only edit your own profile.";
                return RedirectToAction("Profile");
            }

            
            var customerToUpdate = await _customerService.GetByIdAsync(dto.CustomerId);
            if (customerToUpdate == null)
            {
                TempData["Error"] = "Customer not found.";
                return View(dto);
            }

           
            customerToUpdate.Email = dto.Email;
            customerToUpdate.PhoneNumber = dto.PhoneNumber;
            customerToUpdate.EmploymentStatus = dto.EmploymentStatus;
            customerToUpdate.AnnualIncome = dto.AnnualIncome;


            if (customerToUpdate.Email != dto.Email)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.Email = dto.Email;
                    user.UserName = dto.Email; 
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        TempData["Error"] = "Failed to update email in user account.";
                        return View(dto);
                    }
                }
            }


            var newPassword = Request.Form["NewPassword"].ToString();
            var confirmPassword = Request.Form["ConfirmNewPassword"].ToString();
            
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["Error"] = "New passwords do not match.";
                    return View(dto);
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                    if (!resetPasswordResult.Succeeded)
                    {
                        TempData["Error"] = "Failed to change password: " + string.Join(", ", resetPasswordResult.Errors.Select(e => e.Description));
                        return View(dto);
                    }
                }
            }

            var updated = await _customerService.UpdateAsync(dto.CustomerId, customerToUpdate);
            if (updated == null)
            {
                TempData["Error"] = "Failed to update profile.";
                return View(dto);
            }

            TempData["ProfileSuccess"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Dashboard(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return View("~/Views/Customer/Dashboard.cshtml");
        }
    }
}
