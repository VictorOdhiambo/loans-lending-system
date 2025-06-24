namespace LoanManagementApp.DTOs
{
    public class NotificationSendRequest
    {
        public string? NotificationType { get; set; }
        public string? Channel { get; set; }
        public Dictionary<string, string>? Data { get; set; }
      
    }
}
