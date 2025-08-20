using LoanApplicationService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LoanApplicationService.Core.Repository
{
    public class ApplicationUserRole : IdentityUserRole<Guid> { }

    public class LoanApplicationServiceDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public LoanApplicationServiceDbContext(DbContextOptions options) : base(options)
        {
        }

        // DbSets
        public DbSet<LoanProduct> LoanProducts { get; set; } = default!;
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = default!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = default!;
        public DbSet<LoanCharge> LoanCharges { get; set; } = default!;
        public DbSet<LoanChargeMapper> LoanChargeMapper { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = default!;
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<LoanPenalty> LoanPenalties { get; set; } = default!;
        public DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules { get; set; } = default!;
        public DbSet<ApplicationRole> ApplicationRoles { get; set; } = default!;
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; } = default!;
        public DbSet<Transactions> Transactions { get; set; } = default!;
        public DbSet<AuditTrail> AuditTrail { get; set; } = default!;

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

            // Configure Identity relationships to prevent cascade delete conflicts
            // Note: Identity handles role relationships automatically through ApplicationUserRole

            // Configure Identity UserRole relationship
            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne<ApplicationRole>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account LoanApplication
            modelBuilder.Entity<Account>()
                .HasOne(a => a.LoanApplication)
                .WithOne(l => l.Account)
                .HasForeignKey<Account>(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account Customer
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanApplication Customer
            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.LoanApplications)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Map LoanCharge-LoanProduct
            modelBuilder.ApplyConfiguration(new LoanChargeMapConfiguration());

            // Global soft delete filters
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

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.NationalId)
                .IsUnique();

            modelBuilder.Entity<LoanCharge>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Customer>()
                .Property(c => c.AnnualIncome)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AuditTrail>()
                .ToTable("AuditTrail");

            // Strongly-typed AuditTrail relationships using explicit FK properties
            modelBuilder.Entity<AuditTrail>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditTrail>()
                .HasOne(a => a.LoanApplication)
                .WithMany(l => l.AuditTrails)
                .HasForeignKey(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditTrail>()
                .HasOne(a => a.Account)
                .WithMany(acc => acc.AuditTrails)
                .HasForeignKey(a => a.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditTrail>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
