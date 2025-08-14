using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LendingWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetAuditTrailByApplicationId(int applicationId)
        {
            var auditTrail = await _auditService.GetAuditTrailByApplicationIdAsync(applicationId);
            return Ok(auditTrail);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllAuditTrails()
        {
            var auditTrails = await _auditService.GetAllAuditTrailsAsync();
            return Ok(auditTrails);
        }
    }
}
