using LendingApp.Models;
using LoanManagementApp.Data;
using LoanManagementApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LendingApp.Services;

namespace LendingApp.Controllers
{
    public class NotificationTemplateController : Controller
    {
        private readonly NotificationTemplateService _service;

        public NotificationTemplateController(NotificationTemplateService service)
        {
            _service = service;
        }

        // GET: /NotificationTemplate
        public async Task<IActionResult> Index()
        {
            var dtos = await _service.GetAllAsync();
            return View(dtos);
        }

        // GET: api/NotificationTemplate
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<IActionResult> GetTemplates()
        {
            var dtos = await _service.GetAllAsync();
            return Ok(dtos);
        }

        // POST: api/NotificationTemplate
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> CreateTemplate(NotificationTemplateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return Ok(created);
        }

        // PUT: api/NotificationTemplate/{id}
        [HttpPut]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, NotificationTemplateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE: api/NotificationTemplate/{id}
        [HttpDelete]
        [Route("api/[controller]/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
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
        public async Task<IActionResult> Create([Bind("NotificationType,Channel,Subject,BodyText")] NotificationTemplateDto dto)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: NotificationTemplate/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var template = await _service.GetByIdAsync(id.Value);
            if (template == null) return NotFound();
            var dto = _service.ToDto(template);
            return View(dto);
        }

        // POST: NotificationTemplate/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TemplateId,NotificationType,Channel,Subject,BodyText")] NotificationTemplateDto dto)
        {
            if (ModelState.IsValid)
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null) return NotFound();
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: NotificationTemplate/Delete/
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var template = await _service.GetByIdAsync(id.Value);
            if (template == null) return NotFound();
            var dto = _service.ToDto(template);
            return View(dto);
        }

        // POST: NotificationTemplate/Delete/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
