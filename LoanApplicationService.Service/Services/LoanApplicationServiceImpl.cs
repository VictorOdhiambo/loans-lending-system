using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Service.Services
{
    public class LoanApplicationServiceImpl : ILoanApplicationService
    {
        private readonly LoanApplicationServiceDbContext _context;
        private readonly IMapper _mapper;

        public LoanApplicationServiceImpl(LoanApplicationServiceDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            return await _context.SaveChangesAsync() > 0;
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

            application.Status = (int)LoanStatus.Approved;
            application.ApprovedAmount = approvedAmount  ;
            application.DecisionDate = DateTimeOffset.UtcNow;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.ApprovedBy = ApprovedBy;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectAsync(int applicationId)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;

            application.Status = (int)LoanStatus.Rejected;
            application.DecisionDate = DateTimeOffset.UtcNow;
            application.UpdatedAt = DateTimeOffset.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CloseAsync(int applicationId, string decisionNotes)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;
            application.Status = (int)LoanStatus.Closed;
            application.DecisionNotes = decisionNotes;
            application.DecisionDate = DateTimeOffset.UtcNow;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<LoanApplicationDto>> GetByCustomerIdAsync(int customerId)
        {
            var applications = await _context.LoanApplications
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<LoanApplicationDto>>(applications);
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
            application.Status = (int)LoanStatus.Disbursed;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            return await _context.SaveChangesAsync() > 0;
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

        
    }
}