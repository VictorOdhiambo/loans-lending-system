using LendingApp.Models;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using LoanManagementApp.DTOs;

namespace LendingApp.Services
{
    public class NotificationSenderService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSettings _emailSettings;

        public NotificationSenderService(ApplicationDbContext context, IOptions<EmailSettings> emailOptions)
        {
            _context = context;
            _emailSettings = emailOptions.Value;
        }

        public async Task<string> SendNotificationAsync(string notificationType, string channel, Dictionary<string, string> data)
        {
            // 1. Find the template
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.NotificationType == notificationType && t.Channel == channel);

            if (template == null)
            {
                return $"Template not found for type '{notificationType}' and channel '{channel}'";
            }

            // 2. Replace placeholders
            string processedBody = ReplacePlaceholders(template.BodyText ?? string.Empty, data);
            string processedSubject = ReplacePlaceholders(template.Subject ?? string.Empty, data);
            string recipientEmail = data.GetValueOrDefault("Email", "inbox@example.com");

            // 3. Send actual email 
            if (string.Equals(channel, "email", StringComparison.OrdinalIgnoreCase))
            {
                await SendEmailAsync(recipientEmail, processedSubject, processedBody);
            }

            return $"Notification sent to {recipientEmail}";
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            {
                throw new InvalidOperationException("FromEmail in EmailSettings cannot be null or empty.");
            }

            var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName ?? string.Empty),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mail.To.Add(to);

            await client.SendMailAsync(mail);
        }

        private string ReplacePlaceholders(string text, Dictionary<string, string> data)
        {
            return Regex.Replace(text, @"\{\{(.*?)\}\}", match =>
            {
                var key = match.Groups[1].Value.Trim();
                return data.ContainsKey(key) ? data[key] : $"{{{{{key}}}}}";
            });
        }

        public NotificationDto ToDto(Notification notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                NotificationType = notification.NotificationType,
                Channel = notification.Channel,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                SentAt = notification.SentAt,
                Recipient = notification.Recipient
            };
        }

        public async Task<List<NotificationDto>> GetAllNotificationsAsync()
        {
            var notifications = await _context.Notifications.ToListAsync();
            return notifications.Select(ToDto).ToList();
        }
    }
}
