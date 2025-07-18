using LoanApplicationService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanApplicationService.Core.Repository
{
    public class LoanApplicationServiceDbContext(DbContextOptions<LoanApplicationServiceDbContext> options) : DbContext(options)
    {
        // ✅ DbSets
        public DbSet<LoanProduct> LoanProducts { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = default!;
        public DbSet<LoanCharge> LoanCharges { get; set; } = default!;
        public DbSet<LoanChargeMapper> LoanChargeMapper { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = default!;
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Repayment> Repayments { get; set; } = default!;

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

            // 🔗 Account ↔ LoanApplication
            modelBuilder.Entity<Account>()
                .HasOne(a => a.LoanApplication)
                .WithOne(l => l.Account)
                .HasForeignKey<Account>(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔗 Account ↔ Customer
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔗 LoanApplication ↔ Customer
            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.LoanApplications)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔁 Map LoanCharge-LoanProduct
            modelBuilder.ApplyConfiguration(new LoanChargeMapConfiguration());

            // 🧼 Global soft delete filters
            modelBuilder.Entity<LoanProduct>()
                .Property(p => p.IsDeleted).HasDefaultValue(false);
            modelBuilder.Entity<LoanProduct>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<LoanCharge>()
                .Property(p => p.IsDeleted).HasDefaultValue(false);
            modelBuilder.Entity<LoanCharge>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Customer>()
                .Property(c => c.IsDeleted).HasDefaultValue(false);
            modelBuilder.Entity<Customer>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<LoanCharge>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
