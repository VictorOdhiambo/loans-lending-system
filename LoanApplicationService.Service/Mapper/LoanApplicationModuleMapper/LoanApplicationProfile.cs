using LoanApplicationService.CrossCutting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanApplicationService.Service.DTOs.LoanApplicationModule;
using AutoMapper;
using LoanApplicationService.Core.Models;

namespace LoanApplicationService.Service.Mapper.LoanApplicationModuleMapper
{
    public class LoanApplicationProfile : Profile
    {
        public LoanApplicationProfile()
        {
            CreateMap<LoanApplicationDto, LoanApplication>();

            CreateMap<LoanApplication, LoanApplicationDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (LoanStatus)src.Status));
        }
    }
}