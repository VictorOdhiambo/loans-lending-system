using LendingApp.Models;
using LoanManagementApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementApp.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        } 

        // GET: Notification
        public async Task<IActionResult> Index()
        {
            return View(await _context.Notifications.ToListAsync());
        }
    }
}
