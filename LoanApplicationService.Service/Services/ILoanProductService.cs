using System;
using LoanApplicationService.Service.DTOs.LoanModule;

namespace LoanApplicationService.Service.Services;
public interface ILoanProductService
{
    Task<List<LoanProductDto>> GetAllProducts();
    Task<LoanProductDto> GetLoanProductById(int loanProductId);
    Task<LoanProductDto> GetLoanProductWithChargesById(int loanProductId);
    Task<bool> AddLoanProduct(LoanProductDto loanProductDto);
    Task<bool> ModifyLoanProduct(int loanProductId, LoanProductDto loanProductDto);

    Task<bool> DeleteLoanProduct(int loanProductId);
}
