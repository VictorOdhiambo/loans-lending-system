namespace LendingApp.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int CustomerId { get; set; }
        public string? NotificationType { get; set; }
        public string? Channel { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; } 
        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Success { get; set; } 
        public string? ErrorMessage { get; set; }
        public string? Recipient { get; set; }
    }
}
