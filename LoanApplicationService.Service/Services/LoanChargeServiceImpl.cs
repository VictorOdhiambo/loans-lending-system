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
            var loanCharge = await _context.LoanCharges.FindAsync(loanChargeDto.LoanChargeId);
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

        public async Task<IEnumerable<LoanChargeDto>> GetAllChargesForLoanProduct(int loanProductId)
        {
            var loanCharges = await _context.LoanChargeMapper
        .Where(x => x.LoanProductId == loanProductId) 
        .Select(x => new LoanChargeDto
        {
            Id = x.LoanChargeId,
            Name = x.LoanCharge.Name,
            Amount = x.LoanCharge.Amount,
            Description = x.LoanCharge.Description,
            IsPenalty = x.LoanCharge.IsPenalty,
            IsUpfront = x.LoanCharge.IsUpfront,
        })
        .ToListAsync();

            return loanCharges;


        }

        public async Task<bool> AddChargeToProduct(LoanChargeMapperDto loanChargeMapperDto)
        {
           var LoanChargeMap = _mapper.Map<LoanChargeMapperDto, LoanChargeMapper>(loanChargeMapperDto);
            _context.LoanChargeMapper.Add(LoanChargeMap);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<LoanChargeDto>> GetUpFrontCharges(int loanProductId)
        {
            var charges = await _context.LoanChargeMapper
                .Include(x => x.LoanCharge) // This is critical
                .Where(x => x.LoanProductId == loanProductId && x.LoanCharge.IsUpfront)
                .Select(x => new LoanChargeDto
                {
                    Id = x.LoanCharge.Id,
                    Name = x.LoanCharge.Name,
                    Description = x.LoanCharge.Description,
                    IsPenalty = x.LoanCharge.IsPenalty,
                    IsUpfront = x.LoanCharge.IsUpfront,
                    Amount = x.LoanCharge.Amount,
                    IsPercentage = x.LoanCharge.IsPercentage
                })
                .ToListAsync();

            return charges; 
        }

    }
}