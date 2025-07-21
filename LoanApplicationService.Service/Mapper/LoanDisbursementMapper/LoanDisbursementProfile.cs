using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.LoanDisbursement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanApplicationService.Service.Mapper.LoanDisbursementMapper
{
    public class LoanDisbursementProfile :Profile
    {
        public LoanDisbursementProfile()
        {
            CreateMap<LoanWithdawalDto, LoanWithdrawal>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status ?? "Pending"));
            CreateMap<LoanWithdrawal, LoanWithdawalDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}
