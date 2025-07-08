using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public interface INotificationSenderService
    {
        Task<string> SendNotificationAsync(string notificationHeader, string channel, Dictionary<string, string> data);
        Task<List<LoanManagementApp.DTOs.NotificationDto>> GetAllNotificationsAsync();
        Task<string> ResendNotificationAsync(int notificationId);
    }
}
