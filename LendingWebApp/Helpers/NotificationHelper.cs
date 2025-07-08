using LendingApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LendingWebApp.Helpers
{
    public static class NotificationHelper
    {
        public static IEnumerable<SelectListItem> GetChannelOptions(string? selectedValue = null)
        {
            return NotificationChannelExtensions.GetAllChannels()
                .Select(channel => new SelectListItem
                {
                    Value = channel.ToValue(),
                    Text = channel.ToDisplayName(),
                    Selected = selectedValue == channel.ToValue()
                });
        }

        public static string GetChannelDisplayName(string? channelValue)
        {
            if (string.IsNullOrEmpty(channelValue))
                return string.Empty;

            try
            {
                var channel = NotificationChannelExtensions.FromValue(channelValue);
                return channel.ToDisplayName();
            }
            catch
            {
                return channelValue;
            }
        }

        public static IEnumerable<SelectListItem> GetNotificationHeaderOptions(string? selectedValue = null)
        {
            var options = new[]
            {
                "Account Created",
                "Loan Application Received",
                "Loan Disbursed",
                "Loan Rejected",
                "Loan Overdue",
                "Loan Fully Repaid",
                "Payment Reminder" 
            };
            return options.Select(opt => new SelectListItem
            {
                Value = opt,
                Text = opt,
                Selected = selectedValue == opt
            });
        }
    }
} 