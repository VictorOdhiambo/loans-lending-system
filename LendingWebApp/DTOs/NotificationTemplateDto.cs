using System.ComponentModel.DataAnnotations;

namespace LoanManagementApp.DTOs
{
    public class NotificationTemplateDto
    {
        public int TemplateId { get; set; }
        [Required]
        public string NotificationType { get; set; }
        [Required]
        public string Channel { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string BodyText { get; set; }
    }
}
