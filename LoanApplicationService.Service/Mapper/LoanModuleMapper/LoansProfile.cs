using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanModule;
namespace LoanApplicationService.Service.Mapper.LoanModuleMapper;

public class LoansProfile : Profile
{
    public LoansProfile()
    {
        CreateMap<LoanProductDto, LoanProduct>(); 

        CreateMap<LoanProduct, LoanProductDto>()  
            .ForMember(dest => dest.LoanProductTypeDescription, opt => opt.MapFrom(src =>
                Enum.IsDefined(typeof(LoanProductType), src.LoanProductType)
                    ? EnumHelper.GetDescription((LoanProductType)src.LoanProductType)
                    : string.Empty))
            .ForMember(dest => dest.PaymentFrequencyDescription, opt => opt.MapFrom(src =>
                Enum.IsDefined(typeof(PaymentFrequency), src.PaymentFrequency)
                    ? EnumHelper.GetDescription((PaymentFrequency)src.PaymentFrequency)
                    : string.Empty));



        CreateMap<LoanCharge, LoanChargeDto>().ReverseMap();

        CreateMap<LoanChargeMapperDto, LoanChargeMapper>()
            .ForMember(dest => dest.LoanProductId, opt => opt.MapFrom(src => src.LoanProductId))
            .ForMember(dest => dest.LoanChargeId, opt => opt.MapFrom(src => src.LoanChargeId))
            .ReverseMap();         
    }
}








