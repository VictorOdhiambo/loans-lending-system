using LoanApplicationService.Core.Models;
using LoanManagementApp.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public interface INotificationTemplateService
    {
        NotificationTemplateDto ToDto(NotificationTemplate template);
        NotificationTemplate ToEntity(NotificationTemplateDto dto);
        Task<List<NotificationTemplateDto>> GetAllAsync();
        Task<NotificationTemplate?> GetByIdAsync(int id);
        Task<NotificationTemplateDto> CreateAsync(NotificationTemplateDto dto);
        Task<NotificationTemplateDto?> UpdateAsync(int id, NotificationTemplateDto dto);
        Task<bool> DeleteAsync(int id);
    }
} 