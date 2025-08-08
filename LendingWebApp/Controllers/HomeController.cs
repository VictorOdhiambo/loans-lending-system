using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILoanApplicationService _loanApplicationService;
        private readonly ICustomerService _customerService;
        private readonly INotificationSenderService _notificationService;
        private readonly LoanApplicationServiceDbContext _dbContext;

        public HomeController(
            IUserService userService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILoanApplicationService loanApplicationService,
            ICustomerService customerService,
            INotificationSenderService notificationService,
            LoanApplicationServiceDbContext dbContext)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
            _loanApplicationService = loanApplicationService;
            _customerService = customerService;
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginDto { Email = "", Password = "" });
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(loginDto);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, true, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(loginDto);
            }

            // Set welcome message
            var role = "";
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                role = "Super Admin";
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
                role = "Admin";
            else if (await _userManager.IsInRoleAsync(user, "Customer"))
                role = "Customer";
            else
                role = "User";

            TempData["WelcomeMessage"] = $"Welcome back, {user.UserName}! You are logged in as {role}.";

            // Redirect by user roles using Identity's built-in role system
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                return RedirectToAction("Dashboard");
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("AdminDashboard");
            }
            else if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                return RedirectToAction("CustomerDashboard");
            }
            else
            {
                // Fallback: redirect to Index if no role is assigned
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Dashboard()
        {
            // Get dashboard statistics
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            
            // Get loan application statistics
            var allApplications = await _loanApplicationService.GetAllAsync();
            var applicationsList = allApplications.ToList();
            
            // Calculate loan disbursed (sum of approved amounts for disbursed loans)
            var totalLoanDisbursed = applicationsList
                .Where(a => a.Status == LoanStatus.Disbursed)
                .Sum(a => a.ApprovedAmount);
            
            // Count pending applications
            var pendingApplications = applicationsList.Count(a => a.Status == LoanStatus.Pending);
            
            // Count approved applications
            var approvedApplications = applicationsList.Count(a => a.Status == LoanStatus.Approved);
            
            // Count rejected applications
            var rejectedApplications = applicationsList.Count(a => a.Status == LoanStatus.Rejected);
            
            // Count overdue loans (this would need more complex logic based on payment schedules)
            var overdueLoans = 0; 
            
            // Calculate repayment rate (this would need more complex logic)
            var loanRepaymentRate = 0; 
            
            // Get new notifications count (unread notifications from last 7 days)
            var newMessages = 0;
            try
            {
                newMessages = await _dbContext.Notifications
                    .Where(n => !n.IsRead && n.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();
            }
            catch
            {
                // If notifications table doesn't exist or has issues, default to 0
                newMessages = 0;
            }
            
            ViewBag.TotalLoanDisbursed = totalLoanDisbursed;
            ViewBag.PendingApplications = pendingApplications;
            ViewBag.ApprovedApplications = approvedApplications;
            ViewBag.RejectedApplications = rejectedApplications;
            ViewBag.OverdueLoans = overdueLoans;
            ViewBag.LoanRepaymentRate = loanRepaymentRate;
            ViewBag.NewMessages = newMessages;
            
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            // Get admin dashboard statistics
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            
            // Get loan application statistics
            var allApplications = await _loanApplicationService.GetAllAsync();
            var applicationsList = allApplications.ToList();
            
            // Calculate loan disbursed (sum of approved amounts for disbursed loans)
            var totalLoanDisbursed = applicationsList
                .Where(a => a.Status == LoanStatus.Disbursed)
                .Sum(a => a.ApprovedAmount);
            
            // Count pending applications
            var pendingApplications = applicationsList.Count(a => a.Status == LoanStatus.Pending);
            
            // Count approved applications
            var approvedApplications = applicationsList.Count(a => a.Status == LoanStatus.Approved);
            
            // Count rejected applications
            var rejectedApplications = applicationsList.Count(a => a.Status == LoanStatus.Rejected);
            
            // Count overdue loans 
            var overdueLoans = 0; 
            
            // Calculate repayment rate 
            var loanRepaymentRate = 0; 
            
            // Get new notifications count 
            var newMessages = 0;
            try
            {
                newMessages = await _dbContext.Notifications
                    .Where(n => !n.IsRead && n.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();
            }
            catch
            {
                // If notifications table doesn't exist or has issues, default to 0
                newMessages = 0;
            }
            
            ViewBag.TotalLoanDisbursed = totalLoanDisbursed;
            ViewBag.PendingApplications = pendingApplications;
            ViewBag.ApprovedApplications = approvedApplications;
            ViewBag.RejectedApplications = rejectedApplications;
            ViewBag.OverdueLoans = overdueLoans;
            ViewBag.LoanRepaymentRate = loanRepaymentRate;
            ViewBag.NewMessages = newMessages;
            
            return View();
        }

        [Authorize(Roles = "Customer,Admin,SuperAdmin")]
        public async Task<IActionResult> CustomerDashboard()
        {
            // If user is Admin or SuperAdmin, redirect to their appropriate dashboard
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminDashboard");
            }
            else if (User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("Dashboard");
            }

            // Only customers should reach this point
            var customer = await _customerService.GetByEmailAsync(User.Identity.Name);
            if (customer == null)
            {
                return NotFound();
            }

            var loanApplications = await _loanApplicationService.GetByCustomerIdAsync(customer.CustomerId);
            var applicationsList = loanApplications.ToList();

            // Calculate statistics
            var totalApplications = applicationsList.Count;
            var activeLoans = applicationsList.Count(a => a.Status == LoanStatus.Approved || a.Status == LoanStatus.Disbursed);
            var totalBorrowed = applicationsList
                .Where(a => a.Status == LoanStatus.Approved || a.Status == LoanStatus.Disbursed)
                .Sum(a => a.ApprovedAmount);
            var availableCredit = 10000; // This should be calculated based on customer's credit limit

            // Get recent applications 
            var recentApplications = applicationsList
                .OrderByDescending(a => a.ApplicationDate)
                .Take(5)
                .ToList();

            // Pass data to view
            ViewBag.TotalApplications = totalApplications;
            ViewBag.ActiveLoans = activeLoans;
            ViewBag.TotalBorrowed = totalBorrowed;
            ViewBag.AvailableCredit = availableCredit;
            ViewBag.RecentApplications = recentApplications;
            ViewBag.CustomerId = customer.CustomerId;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public IActionResult FAQ()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DebugRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleList = string.Join(", ", roles);
            
            return Content($"User: {user.Email}, Roles: {roleList}, IsAuthenticated: {User.Identity.IsAuthenticated}");
        }
    }

}
