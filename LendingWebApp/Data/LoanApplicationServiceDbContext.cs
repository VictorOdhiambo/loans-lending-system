using Loan_application_service.Models;
using Microsoft.EntityFrameworkCore;

namespace Loan_application_service.Data
{
    public class LoanApplicationServiceDbContext : DbContext
    {
        public LoanApplicationServiceDbContext (DbContextOptions<LoanApplicationServiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<LoanProduct> LoanProducts { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<LoanApplication> LoanApplication { get; set; } = default!;
    }
}
