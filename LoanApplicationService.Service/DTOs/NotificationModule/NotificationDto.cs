namespace LoanManagementApp.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class NotificationDto
    {
        public int NotificationId { get; set; }
        [Display(Name = "Notification Header")]
        public string? NotificationHeader { get; set; }
        public string? Channel { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? SentAt { get; set; }
        public string? Recipient { get; set; }
        public string? SentAtFormatted => SentAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
        public bool Success { get; set; }
        public string Status => Success ? "Sent" : (IsRead ? "Read" : "Failed");
    }
}    