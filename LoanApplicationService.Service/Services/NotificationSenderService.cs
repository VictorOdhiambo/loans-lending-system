﻿using LoanApplicationService.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using LoanManagementApp.DTOs;
using LoanManagementApp.Models;
using LoanApplicationService.Core.Models;

namespace LoanApplicationService.Service.Services
{
    public class NotificationSenderService : INotificationSenderService
    {
        private readonly LoanApplicationServiceDbContext _context;
        private readonly EmailSettings _emailSettings;

        public NotificationSenderService(LoanApplicationServiceDbContext context, IOptions<EmailSettings> emailOptions)
        {
            _context = context;
            _emailSettings = emailOptions.Value;
        }

        public async Task<string> SendNotificationAsync(string notificationHeader, string channel, Dictionary<string, string> data)
        {
            Console.WriteLine($"[Notification] Attempting to send notification. Header: {notificationHeader}, Channel: {channel}, Data: {string.Join(", ", data.Select(kv => kv.Key + ":" + kv.Value))}");
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.NotificationHeader == notificationHeader && t.Channel == channel);

            string recipientEmail = data.GetValueOrDefault("Email", "inbox@example.com");
            int? customerId = null;
           
            var notification = new Notification
            {
                CustomerId = customerId ?? 0,
                NotificationHeader = notificationHeader,
                Channel = channel,
                Title = template?.Subject,
                Message = template?.BodyText,
                IsRead = false,
                SentAt = null,
                CreatedAt = DateTime.UtcNow,
                Success = false,
                ErrorMessage = null,
                Recipient = recipientEmail
            };

            if (template == null)
            {
                notification.ErrorMessage = $"Template not found for header '{notificationHeader}' and channel '{channel}'";
                Console.WriteLine($"[Notification] {notification.ErrorMessage}");
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
                return notification.ErrorMessage;
            }

            string processedBody = ReplacePlaceholders(template.BodyText ?? string.Empty, data);
            string processedSubject = ReplacePlaceholders(template.Subject ?? string.Empty, data);

            try
            {
                if (string.Equals(channel, "email", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[Notification] Sending email to {recipientEmail} with subject '{processedSubject}'");
                    await SendEmailAsync(recipientEmail, processedSubject, processedBody);
                    Console.WriteLine($"[Notification] Email sent to {recipientEmail}");
                }
                notification.Success = true;
                notification.SentAt = DateTime.UtcNow;
                notification.Title = processedSubject;
                notification.Message = processedBody;
            }
            catch (Exception ex)
            {
                notification.ErrorMessage = ex.Message;
                Console.WriteLine($"[Notification] Exception: {ex.Message}\n{ex.StackTrace}");
            }

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification.Success ? $"Notification sent to {recipientEmail}" : $"Failed to send notification: {notification.ErrorMessage}";
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            {
                throw new InvalidOperationException("FromEmail in EmailSettings cannot be null or empty.");
            }

            Console.WriteLine($"[Email] Preparing to send email to {to} via SMTP server {_emailSettings.Host}:{_emailSettings.Port} as {_emailSettings.FromEmail}");
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

            try
            {
                await client.SendMailAsync(mail);
                Console.WriteLine($"[Email] Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email] Failed to send email to {to}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
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
                NotificationHeader = notification.NotificationHeader,
                Channel = notification.Channel,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                SentAt = notification.SentAt,
                Recipient = notification.Recipient,
                Success = notification.Success
            };
        }

        public async Task<List<NotificationDto>> GetAllNotificationsAsync()
        {
            var notifications = await _context.Notifications.ToListAsync();
            return notifications.Select(ToDto).ToList();
        }

        public async Task<string> ResendNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return $"Notification with ID {notificationId} not found.";
            if (string.IsNullOrWhiteSpace(notification.NotificationHeader) || string.IsNullOrWhiteSpace(notification.Channel) || string.IsNullOrWhiteSpace(notification.Recipient))
                return "NotificationHeader, Channel, and Recipient are required to resend.";

            var data = new Dictionary<string, string> { ["Email"] = notification.Recipient };

            // Fetch customer info and add to data dictionary
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == notification.Recipient);
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

                // Fetch most recent loan application for this customer
                var loanApp = await _context.LoanApplications
                    .Where(l => l.CustomerId == customer.CustomerId)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync();
                if (loanApp != null)
                {
                    data["LoanAmount"] = loanApp.RequestedAmount.ToString("F2");
                    data["LoanProductId"] = loanApp.ProductId.ToString();
                    // Add more loan fields as needed
                }
            }

            return await SendNotificationAsync(notification.NotificationHeader, notification.Channel, data);
        }

        public async Task<string> SendNotificationByTemplateId(int templateId, Dictionary<string, string> data)
        {
            var template = await _context.NotificationTemplates.FindAsync(templateId);
            if (template == null)
            {
                var msg = $"[Notification] Template not found for TemplateId '{templateId}'";
                Console.WriteLine(msg);
                return msg;
            }
            return await SendNotificationAsync(template.NotificationHeader, template.Channel, data);
        }

    }
}
