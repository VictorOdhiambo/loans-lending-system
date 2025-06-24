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
        public DbSet<Product> Products { get; set; }


        public DbSet<Customer> customer { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // All relationships are commented out because you're only dealing with Customer

            // modelBuilder.Entity<Account>()
            //     .HasOne(a => a.LoanApplication)
            //     .WithOne(l => l.Account)
            //     .HasForeignKey<Account>(a => a.ApplicationId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // modelBuilder.Entity<Account>()
            //     .HasOne(a => a.Customer)
            //     .WithMany(c => c.Accounts)
            //     .HasForeignKey(a => a.CustomerId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // modelBuilder.Entity<LoanApplication>()
            //     .HasOne(l => l.Customer)
            //     .WithMany(c => c.LoanApplications)
            //     .HasForeignKey(l => l.CustomerId)
            //     .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
