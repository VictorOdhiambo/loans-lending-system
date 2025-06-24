using Loan_application_service.Models;
using Microsoft.EntityFrameworkCore;

namespace Loan_application_service.Data
{
    public class LoanApplicationServiceDbContext : DbContext
    {
        public LoanApplicationServiceDbContext(DbContextOptions<LoanApplicationServiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<LoanProduct> LoanProduct { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<LoanApplication> LoanApplication { get; set; } = default!;
        public DbSet<LoanCharge> LoanCharge { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account to LoanApplication
            modelBuilder.Entity<Account>()
                .HasOne(a => a.LoanApplication)
                .WithOne(l => l.Account)
                .HasForeignKey<Account>(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account to Customer
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanApplication to Customer
            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.LoanApplications)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            //loanproduct to loancharge

            modelBuilder.Entity<LoanCharge>()
                .HasOne(lc => lc.LoanProduct)
                .WithMany(lp => lp.LoanCharges)
                .HasForeignKey(lc => lc.LoanProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
