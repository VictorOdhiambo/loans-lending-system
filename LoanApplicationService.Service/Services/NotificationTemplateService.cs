using LoanApplicationService.Core.Repository;
using LoanManagementApp.DTOs;
using LendingApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LendingApp.Services
{
    public class NotificationTemplateService
    {
        private readonly LoanApplicationServiceDbContext _context;
        private readonly ILogger<NotificationTemplateService> _logger;
        
        public NotificationTemplateService(LoanApplicationServiceDbContext context, ILogger<NotificationTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public NotificationTemplateDto ToDto(NotificationTemplate template)
        {
            return new NotificationTemplateDto
            {
                TemplateId = template.TemplateId,
                NotificationHeader = template.NotificationHeader,
                Channel = template.Channel,
                Subject = template.Subject,
                BodyText = template.BodyText
            };
        }
        public NotificationTemplate ToEntity(NotificationTemplateDto dto)
        {
            return new NotificationTemplate
            {
                NotificationHeader = dto.NotificationHeader,
                Channel = dto.Channel,
                Subject = dto.Subject,
                BodyText = dto.BodyText,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
        public async Task<List<NotificationTemplateDto>> GetAllAsync()
        {
            var templates = await _context.NotificationTemplates.ToListAsync();
            _logger.LogInformation("Retrieved {Count} notification templates from database", templates.Count);
            return templates.Select(ToDto).ToList();
        }
        public async Task<NotificationTemplate?> GetByIdAsync(int id)
        {
            return await _context.NotificationTemplates.FindAsync(id);
        }
        public async Task<NotificationTemplateDto> CreateAsync(NotificationTemplateDto dto)
        {
            var sanitizedHeader = dto.NotificationHeader?.Replace("\r", "").Replace("\n", "");
            var sanitizedChannel = dto.Channel?.Replace("\r", "").Replace("\n", "");
            _logger.LogInformation("Creating new notification template: Header={Header}, Channel={Channel}", 
                sanitizedHeader, sanitizedChannel);
            
            var entity = ToEntity(dto);
            _context.NotificationTemplates.Add(entity);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created notification template with ID: {TemplateId}", entity.TemplateId);
            return ToDto(entity);
        }
        public async Task<NotificationTemplateDto?> UpdateAsync(int id, NotificationTemplateDto dto)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null) return null;
            template.NotificationHeader = dto.NotificationHeader;
            template.Channel = dto.Channel;
            template.Subject = dto.Subject;
            template.BodyText = dto.BodyText;
            template.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return ToDto(template);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null) return false;
            _context.NotificationTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}     