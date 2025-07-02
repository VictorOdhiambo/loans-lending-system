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
            _context.LoanProducts.Add(_mapper.Map<LoanProduct>(loanProductDto));
            return await _context.SaveChangesAsync() >= 0;
        }

        public async Task<List<LoanProductDto>> GetAllProducts()
        {
            var products = await _context.LoanProducts.ToListAsync();
            return _mapper.Map<List<LoanProductDto>>(products);
        }
        public async Task<LoanProductDto> GetLoanProductById(int loanProductId)
        {
            var product = await _context.LoanProducts.FindAsync(loanProductId);
            return product == null
                ? throw new KeyNotFoundException($"Loan product with ID {loanProductId} was not found.")
                : _mapper.Map<LoanProductDto>(product);
        }

        public Task<LoanProductDto> GetLoanProductWithChargesById(int loanProductId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ModifyLoanProduct(int loanProductId, LoanProductDto loanProductDto)
        {
            var product = await _context.LoanProducts.FindAsync(loanProductId);
            if (product != null)
            {
                product.ProductName = loanProductDto.ProductName;
                product.UpdatedAt = DateTime.Now;
                product.InterestRate = loanProductDto.InterestRate;
                product.MinAmount = loanProductDto.MinAmount;
                product.MaxAmount = loanProductDto.MaxAmount;

                return await _context.SaveChangesAsync() >= 0;
            }
            return false;
        }
    }
}
