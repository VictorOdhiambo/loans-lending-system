using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanApplicationService.Service.DTOs.LoanModule;

namespace LoanApplicationService.Service.Services
{
    public interface ILoanChargeService 
    {
        Task<bool> AddLoanCharge(LoanChargeDto loanChargeDto);
        Task<bool> UpdateLoanCharge(LoanChargeDto loanChargeDto);
        Task<bool> DeleteLoanCharge(int id);
        Task<LoanChargeDto?> GetLoanChargeById(int id);
        Task<IEnumerable<LoanChargeDto>> GetAllCharges();
    }
}
