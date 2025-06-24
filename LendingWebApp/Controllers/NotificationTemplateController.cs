using LendingApp.Models;
using LoanManagementApp.Data;
using LoanManagementApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LendingApp.Controllers
{
    public class NotificationTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /NotificationTemplate
        public async Task<IActionResult> Index()
        {
            var templates = await _context.NotificationTemplates.ToListAsync();
            return View(templates);
        }

        // GET: api/NotificationTemplate
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _context.NotificationTemplates.ToListAsync();
            return Ok(templates);
        }

        // POST: api/NotificationTemplate
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> CreateTemplate(NotificationTemplateDto dto)
        {
            var template = new NotificationTemplate
            {
                NotificationType = dto.NotificationType,
                Channel = dto.Channel,
                Subject = dto.Subject,
                BodyText = dto.BodyText,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(template);
        }

        // PUT: api/NotificationTemplate/{id}
        [HttpPut]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, NotificationTemplateDto dto)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            template.NotificationType = dto.NotificationType;
            template.Channel = dto.Channel;
            template.Subject = dto.Subject;
            template.BodyText = dto.BodyText;
            template.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(template); 
        }

        // DELETE: api/NotificationTemplate/{id}
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            _context.NotificationTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: NotificationTemplate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NotificationTemplate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationType,Channel,Subject,BodyText")] NotificationTemplate template)
        {
            if (ModelState.IsValid)
            {
                template.CreatedAt = DateTime.Now;
                template.UpdatedAt = DateTime.Now;
                _context.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(template);
        }

        // GET: NotificationTemplate/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }

        // POST: NotificationTemplate/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TemplateId,NotificationType,Channel,Subject,BodyText,CreatedAt,UpdatedAt")] NotificationTemplate template)
        {
            if (id != template.TemplateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    template.UpdatedAt = DateTime.Now;
                    _context.Update(template);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NotificationTemplates.Any(e => e.TemplateId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }

        // GET: NotificationTemplate/Delete/
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(m => m.TemplateId == id);
            if (template == null)
            {
                return NotFound();
            }

            return View(template);
        }

        // POST: NotificationTemplate/Delete/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template != null)
            {
                _context.NotificationTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
