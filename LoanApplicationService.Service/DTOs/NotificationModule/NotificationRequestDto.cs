namespace LoanManagementApp.DTOs
{
    public class NotificationRequestDto
    {
        public int TemplateId { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Dictionary<string, string>? Placeholders { get; set; }
    }
} 