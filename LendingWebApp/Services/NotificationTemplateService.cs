using LoanManagementApp.Data;
using LoanManagementApp.DTOs;
using LendingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LendingApp.Services
{
    public class NotificationTemplateService
    {
        private readonly ApplicationDbContext _context;
        public NotificationTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public NotificationTemplateDto ToDto(NotificationTemplate template)
        {
            return new NotificationTemplateDto
            {
                TemplateId = template.TemplateId,
                NotificationType = template.NotificationType,
                Channel = template.Channel,
                Subject = template.Subject,
                BodyText = template.BodyText
            };
        }
        public NotificationTemplate ToEntity(NotificationTemplateDto dto)
        {
            return new NotificationTemplate
            {
                NotificationType = dto.NotificationType,
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
            return templates.Select(ToDto).ToList();
        }
        public async Task<NotificationTemplate?> GetByIdAsync(int id)
        {
            return await _context.NotificationTemplates.FindAsync(id);
        }
        public async Task<NotificationTemplateDto> CreateAsync(NotificationTemplateDto dto)
        {
            var entity = ToEntity(dto);
            _context.NotificationTemplates.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }
        public async Task<NotificationTemplateDto?> UpdateAsync(int id, NotificationTemplateDto dto)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null) return null;
            template.NotificationType = dto.NotificationType;
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