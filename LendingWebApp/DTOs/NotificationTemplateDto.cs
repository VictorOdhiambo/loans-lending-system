namespace LoanManagementApp.DTOs
{
    public class NotificationTemplateDto
    {
        public string? NotificationType { get; set; }
        public string? Channel { get; set; }
        public string? Subject { get; set; }
        public string? BodyText { get; set; }
    }
}
