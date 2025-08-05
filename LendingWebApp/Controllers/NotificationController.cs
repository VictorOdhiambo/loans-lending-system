using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanApplicationService.Service.Services;
using Microsoft.Extensions.Logging;
using LoanApplicationService.Web.Services;

namespace LoanApplicationService.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationSenderService _notificationService;
        private readonly EmailService _emailService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationSenderService notificationService, EmailService emailService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
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

            // Fetch customer info and add to data dictionary
            using (var db = new LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<LoanApplicationService.Core.Repository.LoanApplicationServiceDbContext>()))
            {
                var customer = db.Customers.FirstOrDefault(c => c.Email == request.Email);
                if (customer != null)
                {
                    data["FirstName"] = customer.FirstName;
                    data["LastName"] = customer.LastName;
                    data["FullName"] = $"{customer.FirstName} {customer.LastName}";
                    data["PhoneNumber"] = customer.PhoneNumber;
                    data["Address"] = customer.Address ?? string.Empty;
                    data["DateOfBirth"] = customer.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty;
                    data["NationalId"] = customer.NationalId ?? string.Empty;
                    data["EmploymentStatus"] = customer.EmploymentStatus ?? string.Empty;
                    data["AnnualIncome"] = customer.AnnualIncome?.ToString() ?? string.Empty;
                }
            }
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

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var testEmail = "kisueric4@gmail.com";
                var subject = "Test Email from PesaSure";
                var htmlContent = "<h1>This is a test email from PesaSure</h1><p>Email functionality is working!</p>";

                await _emailService.SendEmailAsync(testEmail, subject, htmlContent);
                
                return Ok("Test email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email");
                return StatusCode(500, new { 
                    message = "Failed to send test email",
                    error = ex.Message
                });
            }
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
