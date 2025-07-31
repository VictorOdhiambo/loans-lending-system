using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LoanApplicationService.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        // GET: /Users
        [Authorize(Roles = "SuperAdmin,Admin")]
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
                ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
            }
            ViewBag.CustomerMap = customerMap;

            // Get user roles for each user
            var userRoles = new Dictionary<Guid, string>();
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.UserId.ToString());
                if (appUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(appUser);
                    userRoles[user.UserId] = roles.FirstOrDefault() ?? "No Role";
                }
            }
            ViewBag.UserRoles = userRoles;

            // Get current user information for permission checks
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                ViewBag.CurrentUserRole = currentUserRoles.FirstOrDefault() ?? "No Role";
                ViewBag.CurrentUserId = currentUser.Id;
            }

            return View(users);
        }

        // GET: /Users/Admins
        public async Task<IActionResult> Admins()
        {
            var admins = await _userService.GetAdminsAsync();
            return View(admins);
        }

        // GET: /Users/Details/{id}
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Details(Guid id)
        {
            var users = await _userService.GetAllUsersAsync();
            var inactiveUsers = await _userService.GetInactiveUsersAsync();
            var user = users.Concat(inactiveUsers).FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();
            
            // Get user role using Identity
            var appUser = await _userManager.FindByIdAsync(id.ToString());
            if (appUser != null)
            {
                var roles = await _userManager.GetRolesAsync(appUser);
                ViewBag.UserRole = roles.FirstOrDefault() ?? "No Role";
            }
            
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
            
            // Get user role using Identity
            var appUser = await _userManager.FindByIdAsync(id.ToString());
            if (appUser != null)
            {
                var roles = await _userManager.GetRolesAsync(appUser);
                ViewBag.CurrentUserRole = roles.FirstOrDefault() ?? "No Role";
            }
            
            var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
            if (dbContext != null)
            {
                ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
            }
            return View(user);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string role, bool isActive)
        {
            // Only allow Admins to activate/deactivate Customers
            if (User.IsInRole(LoanApplicationService.CrossCutting.Utils.Role.Admin.ToString()))
            {
                if (role != LoanApplicationService.CrossCutting.Utils.Role.Customer.ToString())
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
                TempData["UserSuccess"] = "User activated successfully!";
            }
            else if (activeResult && !isActive)
            {
                TempData["UserSuccess"] = "User deactivated successfully!";
            }
            else
            {
                TempData["UserSuccess"] = "User updated successfully!";
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
            var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
            if (dbContext != null)
            {
                ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
            }
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate ViewBag for the view
                var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
                if (dbContext != null)
                {
                    ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
                }
                return View(userDto);
            }

            var user = new ApplicationUser
            {
                Email = userDto.Email,
                UserName = userDto.Username,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, userDto.Password ?? string.Empty);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                // Repopulate ViewBag for the view
                var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
                if (dbContext != null)
                {
                    ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
                }
                return View(userDto);
            }

            // Assign role if needed
            if (!string.IsNullOrEmpty(userDto.RoleName))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, userDto.RoleName);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", $"Role assignment failed: {error.Description}");
                    }
                    // Repopulate ViewBag for the view
                    var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
                    if (dbContext != null)
                    {
                        ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
                    }
                    return View(userDto);
                }
            }
            else
            {
                ModelState.AddModelError("", "Role is required for user creation.");
                // Repopulate ViewBag for the view
                var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
                if (dbContext != null)
                {
                    ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
                }
                return View(userDto);
            }

            TempData["UserSuccess"] = "User created successfully!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> FixUserRoles()
        {
            var users = await _userManager.Users.ToListAsync();
            var fixedUsers = new List<string>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    // Check if this is an admin user (you can customize this logic)
                    if (user.Email.Contains("admin") || user.UserName.Contains("admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        fixedUsers.Add($"{user.Email} -> Admin");
                    }
                    else if (user.Email.Contains("super") || user.UserName.Contains("super"))
                    {
                        await _userManager.AddToRoleAsync(user, "SuperAdmin");
                        fixedUsers.Add($"{user.Email} -> SuperAdmin");
                    }
                    else
                    {
                        // Default to Customer for users without roles
                        await _userManager.AddToRoleAsync(user, "Customer");
                        fixedUsers.Add($"{user.Email} -> Customer");
                    }
                }
            }
            
            TempData["UserSuccess"] = $"Fixed roles for {fixedUsers.Count} users: " + string.Join(", ", fixedUsers);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate ViewBag for the view
                var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
                if (dbContext != null)
                {
                    ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
                }
                return View("Create", userDto);
            }

            var user = new ApplicationUser
            {
                Email = userDto.Email,
                UserName = userDto.Username,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, userDto.Password ?? string.Empty);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                // Repopulate ViewBag for the view
                var dbContext = HttpContext.RequestServices.GetService(typeof(LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext)) as LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext;
                if (dbContext != null)
                {
                    ViewBag.RoleList = dbContext.ApplicationRoles.ToList();
                }
                return View("Create", userDto);
            }

            // Assign role if needed
            if (!string.IsNullOrEmpty(userDto.RoleName))
            {
                await _userManager.AddToRoleAsync(user, userDto.RoleName);
            }

            TempData["UserSuccess"] = "User created successfully!";
            return RedirectToAction("Index");
        }
    }
}
