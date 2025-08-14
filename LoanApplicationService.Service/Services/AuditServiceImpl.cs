using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Services
{
    public class AuditServiceImpl : IAuditService
    {
        private readonly LoanApplicationServiceDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditServiceImpl(LoanApplicationServiceDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddLoanApplicationAuditAsync(int applicationId, string action, string oldStatus, string newStatus, string? remarks = null)
        {
            var customerId = await GetCustomerId(applicationId);
            var userId = GetUserId();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            var sql = @"
                INSERT INTO AuditTrail (
                    ApplicationId,
                    Action,
                    OldValues,
                    NewValues,
                    UserId,
                    CustomerId,
                    EntityType,
                    EntityId,
                    CreatedAt,
                    IpAddress,
                    UserAgent
                ) VALUES (
                    @ApplicationId,
                    @Action,
                    @OldValues,
                    @NewValues,
                    @UserId,
                    @CustomerId,
                    @EntityType,
                    @EntityId,
                    @CreatedAt,
                    @IpAddress,
                    @UserAgent
                )";

            await _context.Database.ExecuteSqlRawAsync(sql, 
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@Action", action),
                new SqlParameter("@OldValues", oldStatus),
                new SqlParameter("@NewValues", newStatus),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@EntityType", "LoanApplication"),
                new SqlParameter("@EntityId", applicationId),
                new SqlParameter("@CreatedAt", DateTime.UtcNow),
                new SqlParameter("@IpAddress", ipAddress),
                new SqlParameter("@UserAgent", userAgent));
        }

        public async Task<IEnumerable<AuditTrail>> GetAuditTrailByApplicationIdAsync(int applicationId)
        {
            return await _context.AuditTrail
                .Where(a => a.ApplicationId == applicationId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditTrail>> GetAllAuditTrailsAsync()
        {
            return await _context.AuditTrail
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        private Guid? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
        }

        private async Task<int> GetCustomerId(int applicationId)
        {
            var application = await _context.LoanApplications
                .Where(a => a.ApplicationId == applicationId)
                .Select(a => a.CustomerId)
                .FirstOrDefaultAsync();
            return application;
        }
    }
}
