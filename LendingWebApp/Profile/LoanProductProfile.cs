using AutoMapper;
using Loan_application_service.Models;
using Loan_application_service.DTOs;
namespace Loan_application_service;

public class MappingProfile : Profile
{
    public MappingProfile() {
        CreateMap<LoanProduct, loanproductDto>().ReverseMap();


        CreateMap<LoanCharge, LoanChargeDto>().ReverseMap();
        
     }
}








