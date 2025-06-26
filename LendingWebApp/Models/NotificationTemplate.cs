using System.ComponentModel.DataAnnotations;

namespace LendingApp.Models
{
 
    public class NotificationTemplate
    {
        [Key]
        public int TemplateId { get; set; }
        public string? NotificationType { get; set; }
        public string? Channel { get; set; }
        public string? Subject { get; set; }
        public string? BodyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
