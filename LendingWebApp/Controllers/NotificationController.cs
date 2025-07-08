using LendingApp.Models;
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

        // POST: Notification/Send
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NotificationHeader) || string.IsNullOrWhiteSpace(request.Channel) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("NotificationHeader, Channel, and Email are required.");
            }
            var data = request.Data ?? new Dictionary<string, string>();
            data["Email"] = request.Email; 
            var result = await _notificationService.SendNotificationAsync(request.NotificationHeader, request.Channel, data);
            return Ok(new { message = result });
        }

        [HttpPost]
        public async Task<IActionResult> Resend(int id)
        {
            var result = await _notificationService.ResendNotificationAsync(id);
            if (result.StartsWith("Failed") || result.Contains("not found"))
                return BadRequest(new { message = result });
            return Ok(new { message = result });
        }

        public class SendNotificationRequest
        {
            public string NotificationHeader { get; set; }
            public string Channel { get; set; }
            public string Email { get; set; }
            public Dictionary<string, string>? Data { get; set; }
        }
    }
}
