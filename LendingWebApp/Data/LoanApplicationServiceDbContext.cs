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

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<AuditTrail> AuditTrails { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account ↔ LoanApplication (1:1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.LoanApplication)
                .WithOne(l => l.Account)
                .HasForeignKey<Account>(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account ↔ Customer (M:1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanApplication ↔ Customer (M:1)
            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Customer)
                .WithMany()
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
