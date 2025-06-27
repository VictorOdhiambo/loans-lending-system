using LendingApp.Models;
using LoanManagementApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LendingApp.Services;

namespace LoanManagementApp.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationSenderService _notificationService;
        public NotificationController(NotificationSenderService notificationService)
        {
            _notificationService = notificationService;
        } 

        // GET: Notification
        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            return View(notifications);
        }
    }
}
