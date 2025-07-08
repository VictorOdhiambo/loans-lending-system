using System.ComponentModel.DataAnnotations;
using System.Linq;
using LendingApp.Models;

namespace LoanManagementApp.DTOs
{
    public class NotificationTemplateDto
    {
        public int TemplateId { get; set; }
        
        [Required]
        [Display(Name = "Notification Header")]
        public string? NotificationHeader { get; set; }
        
        [Required]
        [Display(Name = "Channel")]
        [ChannelValidation]
        public string? Channel { get; set; }
        
        [Required]
        public string? Subject { get; set; }
        
        [Required]
        public string? BodyText { get; set; }
    }

    public class ChannelValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string channelValue)
            {
                try
                {
                    NotificationChannelExtensions.FromValue(channelValue);
                    return ValidationResult.Success;
                }
                catch (ArgumentException)
                {
                    var validChannels = string.Join(", ", NotificationChannelExtensions.GetAllChannels().Select(c => c.ToValue()));
                    return new ValidationResult($"Invalid channel. Must be one of: {validChannels}");
                }
            }
            return new ValidationResult("Channel is required.");
        }
    }
}
