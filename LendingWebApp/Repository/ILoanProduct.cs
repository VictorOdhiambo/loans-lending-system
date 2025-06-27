using Loan_application_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Loan_application_service.Repository
{
    public interface  ILoanProductRepository
    {
       IEnumerable<LoanProduct> GetAll();

        LoanProduct GetById(int id);

        Task Insert(LoanProduct item);

        void Update(LoanProduct item);

        void Delete(int id);

        Task SaveAsync ();
    }
}
