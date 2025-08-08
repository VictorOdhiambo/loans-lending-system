using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.RepaymentSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace LoanApplicationService.Service.Mapper.RepaymentScheduleMapper
{
    public class RepaymentScheduleProfile : Profile
    {
        public RepaymentScheduleProfile()
        {
            CreateMap<LoanRepaymentSchedule, RepaymentScheduleDto>()
                .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.InstallmentNumber, opt => opt.MapFrom(src => src.InstallmentNumber))
                .ForMember(dest => dest.PrincipalAmount, opt => opt.MapFrom(src => src.PrincipalAmount))
                .ForMember(dest => dest.InterestAmount, opt => opt.MapFrom(src => src.InterestAmount))
                .ForMember(dest => dest.ScheduledAmount, opt => opt.MapFrom(src => src.ScheduledAmount))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid))
                .ForMember(dest => dest.PaidPrincipal, opt => opt.MapFrom(src => src.PaidPrincipal))
                .ForMember(dest => dest.PaidInterest, opt => opt.MapFrom(src => src.PaidInterest))
                .ForMember(dest => dest.PaidDate, opt => opt.MapFrom(src => src.PaidDate))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
        }
    }
    }

