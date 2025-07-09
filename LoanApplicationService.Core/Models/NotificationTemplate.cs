using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Core.Models
{
 
    public class NotificationTemplate
    {
        [Key]
        public int TemplateId { get; set; }
        public string? NotificationHeader { get; set; }
        public string? Channel { get; set; }
        public string? Subject { get; set; }
        public string? BodyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
