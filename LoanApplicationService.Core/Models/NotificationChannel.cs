namespace LoanApplicationService.Core.Models
{
    public enum NotificationChannel
    {
        Email,
        SMS,
        InApp
    }

    public static class NotificationChannelExtensions
    {
        public static string ToDisplayName(this NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Email => "Email",
                NotificationChannel.SMS => "SMS",
                NotificationChannel.InApp => "In-App",
                _ => channel.ToString()
            };
        }

        public static string ToValue(this NotificationChannel channel)
        {
            return channel.ToString().ToLower();
        }

        public static NotificationChannel FromValue(string value)
        {
            return value?.ToLower() switch
            {
                "email" => NotificationChannel.Email,
                "sms" => NotificationChannel.SMS,
                "inapp" => NotificationChannel.InApp,
                _ => throw new ArgumentException($"Invalid channel value: {value}")
            };
        }

        public static IEnumerable<NotificationChannel> GetAllChannels()
        {
            return Enum.GetValues<NotificationChannel>();
        }
    }
} 