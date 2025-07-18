using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LoanApplicationService.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginDto()); // Views/Home/Index.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginDto loginDto)
        {
            Console.WriteLine($"Login attempt for: {loginDto.Email}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state invalid");
                return View(loginDto);
            }

            var user = await _userService.LoginAsync(loginDto);

            if (user == null)
            {
                Console.WriteLine("Login failed: Invalid email or password");
                ModelState.AddModelError("", "Invalid email or password");
                return View(loginDto);
            }

            // Set session data
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Role", user.Role ?? "");
            HttpContext.Session.SetString("Email", user.Email ?? "");
            Console.WriteLine($"Login success: {user.Email}, {user.Role}, {user.UserId}");

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Index");

            // Get metrics
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LoanApplicationServiceDbContext>();

                // Total Users (Admins + Customers)
                var totalUsers = await db.Users.CountAsync(u => !u.IsDeleted);

                // Total Loan Disbursed (sum of all approved loans)
                var totalLoanDisbursed = await db.LoanApplications
                    .Where(a => a.Status == (int)LoanApplicationService.CrossCutting.Utils.LoanStatus.Approved)
                    .SumAsync(a => (decimal?)a.ApprovedAmount ?? 0);

                // Pending Applications
                var pendingApplications = await db.LoanApplications.CountAsync(a => a.Status == (int)LoanApplicationService.CrossCutting.Utils.LoanStatus.Pending);

                // Overdue Loans (loans with an account whose NextPaymentDate < now and OutstandingBalance > 0)
                var overdueLoans = await db.Accounts.CountAsync(a => a.NextPaymentDate < DateTime.UtcNow && a.OutstandingBalance > 0);

                // Loan Repayment Rate (percentage of loans with OutstandingBalance == 0)
                var totalAccounts = await db.Accounts.CountAsync();
                var fullyRepaid = await db.Accounts.CountAsync(a => a.OutstandingBalance == 0);
                var loanRepaymentRate = totalAccounts > 0 ? (int)Math.Round((double)fullyRepaid / totalAccounts * 100) : 0;

                // New Messages / Tickets (unread notifications)
                var newMessages = await db.Notifications.CountAsync(n => !n.IsRead);

                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalLoanDisbursed = totalLoanDisbursed;
                ViewBag.PendingApplications = pendingApplications;
                ViewBag.OverdueLoans = overdueLoans;
                ViewBag.LoanRepaymentRate = loanRepaymentRate;
                ViewBag.NewMessages = newMessages;
            }

            return View(); // Views/Home/Dashboard.cshtml
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
