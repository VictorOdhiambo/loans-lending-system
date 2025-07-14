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

        public async Task<bool> CreateAsync(LoanApplicationDto dto)
        {
            var application = _mapper.Map<LoanApplication>(dto);
            //get customer ID from jwt token or session
            // check if customer exists
            //var customerExists = await _context.Customer.AnyAsync(c => c.CustomerId == dto.CustomerId);
            // if (!customerExists)
            //  throw new KeyNotFoundException($"Customer with ID {dto.CustomerId} not found.");
            // check if requested amount is within product limits
            var product = await _context.LoanProducts.FindAsync(dto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Loan product with ID {dto.ProductId} not found.");
            if (dto.RequestedAmount < product.MinAmount || dto.RequestedAmount > product.MaxAmount)
                throw new ArgumentOutOfRangeException($"Requested amount must be between {product.MinAmount} and {product.MaxAmount}.");
            // check if term is within product limits
            if (dto.TermMonths < product.MinTermMonths || dto.TermMonths > product.MaxTermMonths)
                throw new ArgumentOutOfRangeException($"Term must be between {product.MinTermMonths} and {product.MaxTermMonths} months.");

            //set interest rate from loan product
            application.InterestRate = product.InterestRate;
            application.CreatedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

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
            if (app == null)
                throw new KeyNotFoundException($"Loan application with ID {applicationId} not found.");

            return _mapper.Map<LoanApplicationDto>(app);
        }






        public async Task<bool> ApproveAsync(int applicationId)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;

            application.Status = (int)LoanStatus.Approved;
            if (application.ApprovedAmount == null)
            {
                application.ApprovedAmount = application.RequestedAmount;
            }

            application.DecisionDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectAsync(int applicationId)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;

            application.Status = (int)LoanStatus.Rejected;
            application.DecisionDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CloseAsync(int applicationId, string decisionNotes)
        {
            var application = await _context.LoanApplications.FindAsync(applicationId);
            if (application == null) return false;
            application.Status = (int)LoanStatus.Closed;
            application.DecisionNotes = decisionNotes;
            application.DecisionDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;
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
            application.DecisionDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}