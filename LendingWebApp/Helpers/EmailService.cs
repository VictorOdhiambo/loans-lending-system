using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace LoanApplicationService.Web.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpSettings? _smtpSettings;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpSettings = _configuration.GetSection("EmailSettings").Get<SmtpSettings>();
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (_smtpSettings == null)
            {
                _logger.LogError("SMTP settings not configured");
                return;
            }
            
            _logger.LogInformation("Preparing to send email to {Email} via SMTP server {SmtpServer}:{Port} as {From}", to, _smtpSettings.SmtpServer, _smtpSettings.Port, _smtpSettings.From);
            Console.WriteLine($"[EmailService] Preparing to send email to {to} via SMTP server {_smtpSettings.SmtpServer}:{_smtpSettings.Port} as {_smtpSettings.From}");
            try
            {
                using var client = new SmtpClient(_smtpSettings.SmtpServer, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = true
                };
                var mail = new MailMessage(_smtpSettings.From, to, subject, body);
                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent successfully to {Email}", to);
                Console.WriteLine($"[EmailService] Email sent successfully to {to}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} using SMTP server {SmtpServer}:{Port} as {From}", to, _smtpSettings.SmtpServer, _smtpSettings.Port, _smtpSettings.From);
                Console.WriteLine($"[EmailService] Failed to send email to {to}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    public class SmtpSettings
    {
        public required string From { get; set; }
        public required string SmtpServer { get; set; }
        public int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
