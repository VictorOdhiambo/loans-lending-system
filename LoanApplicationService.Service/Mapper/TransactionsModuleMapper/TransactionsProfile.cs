using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Service.DTOs.Transactions;

namespace LoanApplicationService.Service.Mapper.TransactionsModuleMapper
{
    public class TransactionsProfile : Profile
    {
        public TransactionsProfile()
        {
            CreateMap<Transactions, TransactionDto>().ReverseMap();

        }
    }
}
