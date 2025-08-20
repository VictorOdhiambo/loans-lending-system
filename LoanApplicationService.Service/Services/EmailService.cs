using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace LoanApplicationService.Web.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _senderEmail = _configuration["EmailSettings:SenderEmail"];
            _senderName = _configuration["EmailSettings:SenderName"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {

                var apiKey = _configuration["EmailSettings:ApiKey"]; apiKey = apiKey ?? string.Empty;


                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_senderEmail, _senderName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);

                var response = await client.SendEmailAsync(msg);
                
                _logger.LogInformation($"Email sent to {toEmail}. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                throw;
            }
        }
    }
}
