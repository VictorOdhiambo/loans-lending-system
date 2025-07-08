using LoanApplicationService.Core.Repository;
using LoanManagementApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoanApplicationService.Service.Services;

namespace LendingApp.Controllers
{
    public class NotificationTemplateController : Controller
    {
        private readonly INotificationTemplateService _service;
        private readonly INotificationSenderService _notificationSenderService;
        private readonly LoanApplicationServiceDbContext _context;
        private readonly ILogger<NotificationTemplateController> _logger;

        public NotificationTemplateController(
            INotificationTemplateService service,
            INotificationSenderService notificationSenderService,
            LoanApplicationServiceDbContext context,
            ILogger<NotificationTemplateController> logger)
        {
            _service = service;
            _notificationSenderService = notificationSenderService;
            _context = context;
            _logger = logger;
        }

        // GET: /NotificationTemplate
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called - retrieving all notification templates");
            var dtos = await _service.GetAllAsync();
            return View(dtos);
        }

        // GET: api/NotificationTemplate
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetTemplates()
        {
            var dtos = await _service.GetAllAsync();
            return Ok(dtos);
        }

        // POST: api/NotificationTemplate
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> CreateTemplate(NotificationTemplateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return Ok(created);
        }

        // PUT: api/NotificationTemplate/{id}
        [HttpPut]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, NotificationTemplateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE: api/NotificationTemplate/{id}
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET: NotificationTemplate/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Create GET action called - showing create form");
            return View();
        }

        // POST: NotificationTemplate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationHeader,Channel,Subject,BodyText")] NotificationTemplateDto dto)
        {
            var sanitizedHeader = SanitizeForLog(dto.NotificationHeader);
            var sanitizedChannel = SanitizeForLog(dto.Channel);
            var sanitizedSubject = SanitizeForLog(dto.Subject);
            _logger.LogInformation("Create POST action called with data: Header={Header}, Channel={Channel}, Subject={Subject}", 
                sanitizedHeader, sanitizedChannel, sanitizedSubject);
            
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid, calling service to create template");
                await _service.CreateAsync(dto);
                _logger.LogInformation("Template created successfully, redirecting to Index");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogWarning("ModelState is invalid: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
            return View(dto);
        }

        // GET: NotificationTemplate/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var template = await _service.GetByIdAsync(id.Value);
            if (template == null) return NotFound();
            var dto = _service.ToDto(template);
            return View(dto);
        }

        // POST: NotificationTemplate/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationHeader,Channel,Subject,BodyText")] NotificationTemplateDto dto)
        {
            if (ModelState.IsValid)
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null) return NotFound();
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: NotificationTemplate/Delete/
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var template = await _service.GetByIdAsync(id.Value);
            if (template == null) return NotFound();
            var dto = _service.ToDto(template);
            return View(dto);
        }

        // POST: NotificationTemplate/Delete/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: api/NotificationTemplate/send
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] LoanManagementApp.DTOs.NotificationRequestDto request)
        {
            if (request == null || request.TemplateId == 0)
                return BadRequest("TemplateId is required.");

            var template = await _service.GetByIdAsync(request.TemplateId);
            if (template == null)
                return BadRequest("Template not found.");

            // Prepare placeholder data
            var data = request.Placeholders ?? new Dictionary<string, string>();

            // Determine recipient
            string recipientEmail = request.Email;
            if (string.IsNullOrWhiteSpace(recipientEmail) && request.UserId.HasValue)
            {
                // Try to get user info from DB if UserId is provided
                // No Users table in context, so fallback to input only
                return BadRequest("User lookup by UserId is not supported in this context. Please provide Email or PhoneNumber.");
            }
            if (!string.IsNullOrWhiteSpace(recipientEmail))
            {
                data["Email"] = recipientEmail;
            }
            else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                data["PhoneNumber"] = request.PhoneNumber;
            }
            else
            {
                return BadRequest("Recipient Email or PhoneNumber is required.");
            }

            // Use template's NotificationHeader and Channel
            var result = await _notificationSenderService.SendNotificationAsync(
                template.NotificationHeader,
                template.Channel,
                data
            );

            if (result.StartsWith("Failed"))
                return BadRequest(result);
            return Ok(new { message = result });
        }

        // GET: api/NotificationTemplate/all-sent
        [HttpGet("all-sent")]
        public async Task<IActionResult> GetAllSentNotifications()
        {
            var notifications = await _notificationSenderService.GetAllNotificationsAsync();
            return Ok(notifications);
        }

        // Helper function to sanitize log input
        private static string SanitizeForLog(string? input)
        {
            if (input == null) return string.Empty;
            var sanitized = input.Replace("\n", " ").Replace("\r", " ").Replace(Environment.NewLine, " ");
            return sanitized;
        }
    }
}
