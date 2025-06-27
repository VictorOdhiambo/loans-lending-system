using Loan_application_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net.WebSockets;
using Loan_application_service.DTOs;

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

        public DbSet<LoanChargeMapper> LoanChargeMapper { get; set; } = default!;

        // Many-to-many configuration for loan products and loan charges.
        public class LoanChargeMapConfiguration : IEntityTypeConfiguration<LoanChargeMapper>
        {
            public void Configure(EntityTypeBuilder<LoanChargeMapper> builder)
            {
                builder.HasKey(s => new { s.LoanChargeId, s.LoanProductId });
                builder.HasOne(ss => ss.LoanProduct)
                    .WithMany(s => s.LoanChargeMap)
                    .HasForeignKey(ss => ss.LoanProductId);
                builder.HasOne(ss => ss.LoanCharge)
                    .WithMany(s => s.LoanChargeMap)
                    .HasForeignKey(ss => ss.LoanChargeId);
            }
        }

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

                // Explicitly specify the type argument for ApplyConfiguration
            modelBuilder.ApplyConfiguration(new LoanChargeMapConfiguration());

            

            

            modelBuilder.Entity<LoanProduct>()
                .Property(c => c.RepaymentFrequency)
                .HasConversion<string>();
        }
        public DbSet<Loan_application_service.DTOs.loanproductDto> loanproductDto { get; set; } = default!;
    }
}
