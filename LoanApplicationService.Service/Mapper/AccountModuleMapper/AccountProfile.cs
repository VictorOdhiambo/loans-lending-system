using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.Account;

namespace LoanApplicationService.Service.Mapper.AccountModuleMapper
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountDto, Account>();
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}
