using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.DTOs.LoanModule;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Service.Services
{
    public class LoanProductServiceImpl(LoanApplicationServiceDbContext context, IMapper mapper) : ILoanProductService
    {
        private readonly LoanApplicationServiceDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> AddLoanProduct(LoanProductDto loanProductDto)
        {
            try
            {
                var loanProduct = _mapper.Map<LoanProduct>(loanProductDto);

                loanProduct.CreatedAt = DateTime.UtcNow;
                loanProduct.UpdatedAt = DateTime.UtcNow;
                loanProduct.IsDeleted = false;

                await _context.LoanProducts.AddAsync(loanProduct);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (AutoMapperMappingException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<List<LoanProductDto>> GetAllProducts()
        {
            var products = await _context.LoanProducts.ToListAsync();
            return _mapper.Map<List<LoanProductDto>>(products);
        }
        public async Task<LoanProductDto> GetLoanProductById(int loanProductId)
        {
            var product = await _context.LoanProducts.FindAsync(loanProductId);
            
           if (product == null)
            {
                throw new KeyNotFoundException($"Loan product with ID {loanProductId} was not found.");
            }
            return _mapper.Map<LoanProductDto>(product);
        }


        public async Task<LoanProductDto> GetLoanProductWithChargesById(int loanProductId)
        {
            var product = await _context.LoanProducts
                .Include(lp => lp.LoanCharges)
                .FirstOrDefaultAsync(lp => lp.ProductId == loanProductId);

            return product == null ? null : _mapper.Map<LoanProductDto>(product);
        }


        public async Task<bool> ModifyLoanProduct(int loanProductId, LoanProductDto loanProductDto)
        {
            var product = await _context.LoanProducts.FindAsync(loanProductId);
            if (product != null)
            {
                product.ProductName = loanProductDto.ProductName;
                product.LoanProductType = (int)loanProductDto.LoanProductType;
                product.PaymentFrequency = (int)loanProductDto.PaymentFrequency;
                product.MinAmount = loanProductDto.MinAmount;
                product.MaxAmount = loanProductDto.MaxAmount;
                product.InterestRate = loanProductDto.InterestRate;
                product.MinTermMonths = loanProductDto.MinTermMonths;
                product.MaxTermMonths = loanProductDto.MaxTermMonths;
                product.EligibilityCriteria = loanProductDto.EligibilityCriteria;
                product.ProcessingFee = loanProductDto.ProcessingFee;
                product.IsActive = loanProductDto.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }


        public async Task<IEnumerable<LoanChargeDto>> GetAllChargesForLoanProduct(int loanProductId)
        {
            var loanProduct = await _context.LoanProducts
                .Include(lp => lp.LoanCharges)
                .FirstOrDefaultAsync(lp => lp.ProductId == loanProductId);
            if (loanProduct == null) return Enumerable.Empty<LoanChargeDto>();
            return _mapper.Map<IEnumerable<LoanChargeDto>>(loanProduct.LoanCharges);
        }
        public async Task<bool> DeleteLoanProduct(int loanProductId)
        {
            var product = await _context.LoanProducts.FindAsync(loanProductId);
            if (product != null)
            {
                product.IsDeleted = true;
                product.DeletedOn = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new KeyNotFoundException($"Loan product with ID {loanProductId} was not found.");
            }
        }
    }
}
        
   