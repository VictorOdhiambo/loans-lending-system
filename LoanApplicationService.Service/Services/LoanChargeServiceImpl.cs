using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Service.Services
{
    public class LoanChargeServiceImpl(LoanApplicationServiceDbContext context, IMapper mapper) : ILoanChargeService
    {
        private readonly LoanApplicationServiceDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> AddLoanCharge(LoanChargeDto loanChargeDto)
        {
            var loanCharge = _mapper.Map<LoanCharge>(loanChargeDto);
            _context.LoanCharges.Add(loanCharge);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateLoanCharge(LoanChargeDto loanChargeDto)
        {
            var loanCharge = await _context.LoanCharges.FindAsync(loanChargeDto.Id);
            if (loanCharge == null) return false;
            _mapper.Map(loanChargeDto, loanCharge);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLoanCharge(int id)
        {
            var loanCharge = await _context.LoanCharges.FindAsync(id);
            if (loanCharge == null) return false;
            loanCharge.IsDeleted = true; 
             _context.LoanCharges.Update(loanCharge); 
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<LoanChargeDto?> GetLoanChargeById(int id)
        {
            var loanCharge = await _context.LoanCharges.FindAsync(id);
            return loanCharge == null ? null : _mapper.Map<LoanChargeDto>(loanCharge);
        }

        public async Task<IEnumerable<LoanChargeDto>> GetAllCharges()
        {
            var loanCharges = await _context.LoanCharges.ToListAsync();
            var charges = _mapper.Map<IEnumerable<LoanChargeDto>>(loanCharges);
            return charges;
        }

        // Get all charges for a specific loan product using the relationship
        public async Task<IEnumerable<LoanChargeDto>> GetAllChargesForLoanProduct(int loanProductId)
        {
            var loanProduct = await _context.LoanProducts
                .Include(lp => lp.LoanCharges)
                .FirstOrDefaultAsync(lp => lp.ProductId == loanProductId);

            if (loanProduct == null) return Enumerable.Empty<LoanChargeDto>();

            return _mapper.Map<IEnumerable<LoanChargeDto>>(loanProduct.LoanCharges);
        }
    }
}