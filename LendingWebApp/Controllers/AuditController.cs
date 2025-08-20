using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingWebApp.Controllers
{
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        // Web View for Audit Trail
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Index(string? entityType)
        {
            var types = await _auditService.GetDistinctEntityTypesAsync();
            ViewBag.EntityTypes = types;
            ViewBag.SelectedEntityType = entityType;

            IEnumerable<AuditTrail> audits;
            if (!string.IsNullOrWhiteSpace(entityType))
            {
                audits = await _auditService.GetAuditTrailsByEntityTypeAsync(entityType);
            }
            else
            {
                audits = await _auditService.GetAllAuditTrailsAsync();
            }
            return View(audits);
        }

        // API Endpoints
        [HttpGet("api/{applicationId}")]
        public async Task<IActionResult> GetAuditTrailByApplicationId(int applicationId)
        {
            var auditTrail = await _auditService.GetAuditTrailByApplicationIdAsync(applicationId);
            return Ok(auditTrail);
        }

        [HttpGet("api")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllAuditTrails()
        {
            var auditTrails = await _auditService.GetAllAuditTrailsAsync();
            return Ok(auditTrails);
        }
    }
}
