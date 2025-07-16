using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
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
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
