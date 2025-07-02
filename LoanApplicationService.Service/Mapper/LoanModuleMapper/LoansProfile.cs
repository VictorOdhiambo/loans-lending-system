using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.LoanModule;
namespace LoanApplicationService.Service.Mapper.LoanModuleMapper;

public class LoansProfile : Profile
{
    public LoansProfile()
    {
        CreateMap<LoanProduct, LoanProductDto>().ReverseMap();
        CreateMap<LoanCharge, LoanChargeDto>().ReverseMap();
    }
}








