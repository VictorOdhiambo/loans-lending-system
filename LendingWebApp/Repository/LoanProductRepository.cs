using Loan_application_service.Data;
using Loan_application_service.Models;
using Microsoft.EntityFrameworkCore;
using Loan_application_service.Repository;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using Loan_application_service.DTOs;

namespace Loan_application_service.Repository
{
    public class LoanProductRepository : ILoanProductRepository

    {
        private readonly LoanApplicationServiceDbContext _context;

        public LoanProductRepository(LoanApplicationServiceDbContext context)
        {
            _context = context;
        }

        public IEnumerable<LoanProduct> GetAll()
        {
            return _context.LoanProduct.ToList();

        }

        public LoanProduct GetById(int id)
        {

            return _context.LoanProduct.Find(id);

        }

        public void Delete(int id)
        {

            LoanProduct product = _context.LoanProduct.Find(id);

            if (product != null)
            {
                _context.LoanProduct.Remove(product);
            }

        }

        public void Update(LoanProduct product)
        {

            _context.Entry(product).State = EntityState.Modified;

        }

        public async Task Insert(LoanProduct item)
        {
           await _context.LoanProduct.AddAsync(item);

        }

        public async Task SaveAsync()
        {

            await _context.SaveChangesAsync();

        }

    }   
}
