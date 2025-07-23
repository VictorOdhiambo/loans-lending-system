using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LoanApplicationService.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Users
        public async Task<IActionResult> Index(bool showInactive = false)
        {
            var users = showInactive
                ? await _userService.GetInactiveUsersAsync()
                : await _userService.GetAllUsersAsync();

            // Build a map of UserId to CustomerId for all customers
            var customerMap = new Dictionary<Guid, int>();
            var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
            if (dbContext != null)
            {
                var customers = dbContext.Customers.ToList();
                foreach (var customer in customers)
                {
                    customerMap[customer.UserId] = customer.CustomerId;
                }
            }
            ViewBag.CustomerMap = customerMap;

            return View(users);
        }

        // GET: /Users/Admins
        public async Task<IActionResult> Admins()
        {
            var admins = await _userService.GetAdminsAsync();
            return View(admins);
        }

        // GET: /Users/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var users = await _userService.GetAllUsersAsync();
            var inactiveUsers = await _userService.GetInactiveUsersAsync();
            var user = users.Concat(inactiveUsers).FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var users = await _userService.GetAllUsersAsync();
            var inactiveUsers = await _userService.GetInactiveUsersAsync();
            var user = users.Concat(inactiveUsers).FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string role, bool isActive)
        {
            // Only allow Admins to activate/deactivate Customers
            if (User.IsInRole("Admin"))
            {
                if (role != "Customer")
                {
                    return Forbid();
                }
            }

            var roleResult = await _userService.UpdateUserRoleAsync(id, role);
            var activeResult = await _userService.SetUserActiveStatusAsync(id, isActive);
            if (!roleResult && !activeResult)
            {
                TempData["Error"] = "Failed to update user.";
            }
            else if (activeResult && isActive)
            {
                TempData["Success"] = "User activated successfully!";
            }
            else if (activeResult && !isActive)
            {
                TempData["Success"] = "User deactivated successfully!";
            }
            else
            {
                TempData["Success"] = "User updated successfully!";
            }

            // AJAX: return JSON, otherwise redirect
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var role = form["Role"].ToString();
            if (role == "Admin")
            {
                var username = form["Username"].ToString();
                var email = form["Email"].ToString();
                var password = form["Password"].ToString();
                var confirmPassword = form["ConfirmPassword"].ToString();
                if (password != confirmPassword)
                {
                    TempData["Error"] = "Passwords do not match.";
                    return View();
                }
                var userDto = new LoanApplicationService.Service.DTOs.UserModule.UserDTO
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    Role = "Admin",
                    IsActive = true
                };
                var result = await _userService.RegisterAsync(userDto);
                if (!result)
                {
                    TempData["Error"] = "A user with this email already exists.";
                    return View();
                }
                TempData["Success"] = "Admin user created successfully!";
                return RedirectToAction("Index");
            }
            else if (role == "Customer")
            {
                // Redirect to CustomerController's Create action for customer creation
                return RedirectToAction("Create", "Customer");
            }
            TempData["Error"] = "Invalid role selected.";
            return View();
        }
    }
}
