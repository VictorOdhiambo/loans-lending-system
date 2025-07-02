using LoanApplicationService.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace LoanApplicationService.Core.Repository
{
    public class LoanApplicationServiceDbContext(DbContextOptions<LoanApplicationServiceDbContext> options) : DbContext(options)
    {
        public DbSet<LoanProduct> LoanProducts { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = default!;
        public DbSet<LoanCharge> LoanCharges { get; set; } = default!;

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

            modelBuilder.Entity<LoanCharge>();

        }

    }
}
