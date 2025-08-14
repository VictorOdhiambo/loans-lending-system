using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace LoanApplicationService.Service.Services
{
    public class LoanApplicationServiceImpl : ILoanApplicationService
    {
        private readonly LoanApplicationServiceDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoanApplicationServiceImpl(LoanApplicationServiceDbContext context, IMapper mapper, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CreateAsync(LoanApplicationDto loanApplicationDto)
        {
            var application = _mapper.Map<LoanApplication>(loanApplicationDto);
            // Fetch customer
            var customer = await _context.Customers.FindAsync(loanApplicationDto.CustomerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {loanApplicationDto.CustomerId} not found.");
            // Check age
            if (!customer.DateOfBirth.HasValue)
                throw new ArgumentException("Customer date of birth is required for eligibility.");
            var today = DateTime.UtcNow;
            var age = today.Year - customer.DateOfBirth.Value.Year;
            if (customer.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            if (age < 18)
                throw new ArgumentException("Customer must be at least 18 years old to apply for a loan.");
            // Check employment status
            if (string.IsNullOrWhiteSpace(customer.EmploymentStatus))
                throw new ArgumentException("Employment status is required for loan eligibility.");
            // Check annual income
            if (!customer.AnnualIncome.HasValue || customer.AnnualIncome.Value < 0)
                throw new ArgumentException("Annual income must be provided and non-negative for loan eligibility.");
            // check if requested amount is within product limits
            var product = await _context.LoanProducts.FindAsync(loanApplicationDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Loan product with ID {loanApplicationDto.ProductId} not found.");
            if (loanApplicationDto.RequestedAmount < product.MinAmount || loanApplicationDto.RequestedAmount > product.MaxAmount)
                throw new ArgumentOutOfRangeException($"Requested amount must be between {product.MinAmount} and {product.MaxAmount}.");
            // check if term is within product limits
            if (loanApplicationDto.TermMonths < product.MinTermMonths || loanApplicationDto.TermMonths > product.MaxTermMonths)
                throw new ArgumentOutOfRangeException($"Term must be between {product.MinTermMonths} and {product.MaxTermMonths} months.");

            var paymentFrequency = await _context.LoanProducts
                .Where(p => p.ProductId == loanApplicationDto.ProductId)
                .Select(p => p.PaymentFrequency)
                .FirstOrDefaultAsync();
            //set interest rate from loan product
            application.InterestRate = product.InterestRate;
            
            application.CreatedAt = DateTimeOffset.UtcNow;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.PaymentFrequency = paymentFrequency;
            
            await _context.LoanApplications.AddAsync(application);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _auditService.AddLoanApplicationAuditAsync(
                    application.ApplicationId,
                    "Loan application created successfully",
                    "Created",
                    "Pending"
                );
            }

            return result;
        }

        public async Task<IEnumerable<LoanApplicationDto>> GetAllAsync()
        {

            var applications = await _context.LoanApplications
                .Include(a => a.Customer)
                .Include(a => a.LoanProduct)
                .Include(a => a.ProcessedByUser)
                .ToListAsync();

          
            return _mapper.Map<IEnumerable<LoanApplicationDto>>(applications);

        }

        public async Task<LoanApplicationDto> GetByIdAsync(int applicationId)
        {
            var app = await _context.LoanApplications.FindAsync(applicationId);
            if (app != null)
            {

                return _mapper.Map<LoanApplicationDto>(app);
            }
            return null;
        }

        public async Task<bool> ApproveAsync(int applicationId, decimal approvedAmount, Guid ApprovedBy)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;

            var oldStatus = application.Status.ToString();
            application.Status = (int)LoanStatus.Approved;
            application.ApprovedAmount = approvedAmount;
            application.DecisionDate = DateTimeOffset.UtcNow;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.ApprovedBy = ApprovedBy;

            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                await _auditService.AddLoanApplicationAuditAsync(
                    applicationId,
                    $"Loan application approved with amount {approvedAmount}",
                    oldStatus = EnumHelper.GetDescription(LoanStatus.Pending),
                    "Approved"
                );
            }

            return result;
        }

        public async Task<bool> RejectAsync(int applicationId)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;

            var oldStatus = application.Status.ToString();
            application.Status = (int)LoanStatus.Rejected;
            application.UpdatedAt = DateTimeOffset.UtcNow;

            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                await _auditService.AddLoanApplicationAuditAsync(
                    applicationId,
                    "Loan application rejected",
                    oldStatus = EnumHelper.GetDescription(LoanStatus.Pending),
                    "Rejected"
                );
            }

            return result;
        }

        public async Task<bool> CloseAsync(int applicationId, string decisionNotes)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;
            var oldStatus = application.Status.ToString();
            application.Status = (int)LoanStatus.Closed;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.DecisionNotes = decisionNotes;

            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                await _auditService.AddLoanApplicationAuditAsync(
                    applicationId,
                    "Loan application closed",
                    oldStatus = EnumHelper.GetDescription(LoanStatus.Rejected),
                    "Closed"
                );
            }

            return result;
        }

        public async Task<bool> CustomerReject(int applicationId, string reason)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;
            application.Status = (int)LoanStatus.CustomerRejected;
            application.DecisionNotes = reason;
            application.DecisionDate = DateTimeOffset.UtcNow;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DisburseAsync(int applicationId)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;

            // Get the old status as a string
            var oldStatus = EnumHelper.GetDescription(LoanStatus.Approved);
            application.Status = (int)LoanStatus.Disbursed;
            application.UpdatedAt = DateTimeOffset.UtcNow;

            // Calculate monthly payment
            var monthlyPayment = CalculateMonthlyPayment(
                application.ApprovedAmount ?? 0,
                application.InterestRate,
                application.TermMonths
            );

            // Create loan account
            var account = new Account
            {
                CustomerId = application.CustomerId,
                ApplicationId = applicationId,
                AccountNumber = GenerateAccountNumber(),
                AccountType = "Loan",
                PrincipalAmount = application.ApprovedAmount ?? 0,
                AvailableBalance = application.ApprovedAmount ?? 0,
                OutstandingBalance = application.ApprovedAmount ?? 0,
                PaymentFrequency = application.PaymentFrequency,
                InterestRate = application.InterestRate,
                TermMonths = application.TermMonths,
                MonthlyPayment = monthlyPayment,
                Status = (int)AccountStatus.Active,
                DisbursementDate = DateTime.UtcNow,
                MaturityDate = DateTime.UtcNow.AddMonths(application.TermMonths),
                NextPaymentDate = DateTime.UtcNow.AddDays(7),
                Customer = application.Customer,
                LoanApplication = application
            };

            // Add account to context before associating with loan application
            _context.Accounts.Add(account);
            
            // Associate account with loan application
            application.Account = account;

            // Save changes to get AccountId
            var result = await _context.SaveChangesAsync() > 0;
            if (result)
            {
                // Get the current user ID
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                var userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : (Guid?)null;

                // Get customer ID
                var customerId = await _context.LoanApplications
                    .Where(a => a.ApplicationId == applicationId)
                    .Select(a => a.CustomerId)
                    .FirstOrDefaultAsync();

                // Get IP address and user agent
                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

                // Create audit trail
                var sql = @"
                    INSERT INTO AuditTrail (
                        ApplicationId,
                        Action,
                        OldValues,
                        NewValues,
                        UserId,
                        CustomerId,
                        AccountId,
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
                        @AccountId,
                        @EntityType,
                        @EntityId,
                        @CreatedAt,
                        @IpAddress,
                        @UserAgent
                    )";

                // Get the AccountId after saving
                var accountId = account.AccountId;

                await _context.Database.ExecuteSqlRawAsync(sql, 
                    new SqlParameter("@ApplicationId", applicationId),
                    new SqlParameter("@Action", $"Loan disbursed with amount {application.ApprovedAmount}"),
                    new SqlParameter("@OldValues", oldStatus),
                    new SqlParameter("@NewValues", "Disbursed"),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@AccountId", accountId),
                    new SqlParameter("@EntityType", "LoanApplication"),
                    new SqlParameter("@EntityId", applicationId),
                    new SqlParameter("@CreatedAt", DateTime.UtcNow),
                    new SqlParameter("@IpAddress", ipAddress),
                    new SqlParameter("@UserAgent", userAgent));
            }

            return result;
        }

        private decimal CalculateMonthlyPayment(decimal principal, decimal interestRate, int termMonths)
        {
            var monthlyRate = interestRate / 12;
            var numerator = monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, termMonths);
            var denominator = (decimal)Math.Pow(1 + (double)monthlyRate, termMonths) - 1;
            return principal * (numerator / denominator);
        }

        private string GenerateAccountNumber()
        {
            // Generate a unique account number
            return $"L{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        public async Task<bool> UpdateAsync(LoanApplicationDto dto)
        {
            var application = await _context.LoanApplications.FindAsync(dto.ApplicationId);
            if (application == null) return false;
            _mapper.Map(dto, application);
            application.UpdatedAt = DateTimeOffset.UtcNow;
            _context.LoanApplications.Update(application);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<LoanApplicationDto>> GetByCustomerIdAsync(int customerId)
        {
            var applications = await _context.LoanApplications
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<LoanApplicationDto>>(applications);
        }
    }
}