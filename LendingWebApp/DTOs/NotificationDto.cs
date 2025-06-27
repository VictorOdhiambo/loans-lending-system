namespace LoanManagementApp.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string? NotificationType { get; set; }
        public string? Channel { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? SentAt { get; set; }
        public string? Recipient { get; set; }
        public string? SentAtFormatted => SentAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
        public string Status => IsRead ? "Read" : "Unread";
    }
}    