using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Index");

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
