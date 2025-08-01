﻿using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.CrossCutting.Utils;
using LoanApplicationService.Service.DTOs.LoanModule;
using LoanApplicationService.Service.DTOs.UserModule;
using LoanApplicationService.Service.DTOs.CustomerModule;

namespace LoanApplicationService.Service.Mapper.LoanModuleMapper
{
    public class LoansProfile : Profile
    {
        public LoansProfile()
        {
            // LoanProduct mapping
            CreateMap<LoanProductDto, LoanProduct>()
                .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => src.RiskLevel));

            CreateMap<LoanProduct, LoanProductDto>()
                .ForMember(dest => dest.LoanProductTypeDescription, opt => opt.MapFrom(src =>
                    Enum.IsDefined(typeof(LoanProductType), src.LoanProductType)
                        ? EnumHelper.GetDescription((LoanProductType)src.LoanProductType)
                        : string.Empty))
                .ForMember(dest => dest.PaymentFrequencyDescription, opt => opt.MapFrom(src =>
                    Enum.IsDefined(typeof(PaymentFrequency), src.PaymentFrequency)
                        ? EnumHelper.GetDescription((PaymentFrequency)src.PaymentFrequency)
                        : string.Empty))
                .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => src.RiskLevel))
                .ForMember(dest => dest.RiskLevelDescription, opt => opt.MapFrom(src => EnumHelper.GetDescription(src.RiskLevel)));

            // LoanCharge mapping
            CreateMap<LoanCharge, LoanChargeDto>()
                .ForMember(dest => dest.LoanChargeId, opt => opt.MapFrom(src => src.Id));
            CreateMap<LoanChargeDto, LoanCharge>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LoanChargeId));

            // ✅ Users mapping
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
            CreateMap<UserDTO, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));

        CreateMap<LoanChargeMapperDto, LoanChargeMapper>()
            .ForMember(dest => dest.LoanProductId, opt => opt.MapFrom(src => src.LoanProductId))
            .ForMember(dest => dest.LoanChargeId, opt => opt.MapFrom(src => src.LoanChargeId))
            .ReverseMap();         
            // ✅ Customer mapping
            CreateMap<Customer, CustomerDto>().ReverseMap();
        }
    }
}
