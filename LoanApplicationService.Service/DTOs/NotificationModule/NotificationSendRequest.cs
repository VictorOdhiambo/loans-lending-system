namespace LoanManagementApp.DTOs
{
    public class NotificationSendRequest
    {
        public string? NotificationHeader { get; set; }
        public string? Channel { get; set; }
        public Dictionary<string, string>? Data { get; set; }
      
    }
}
