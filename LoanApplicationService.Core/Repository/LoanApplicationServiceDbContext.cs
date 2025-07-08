using LoanApplicationService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net.WebSockets;

namespace LoanApplicationService.Core.Repository;

public class LoanApplicationServiceDbContext(DbContextOptions<LoanApplicationServiceDbContext> options) : DbContext(options)
{
    public DbSet<LoanProduct> LoanProducts { get; set; } = default!;
    public DbSet<Users> Users { get; set; } = default!;
    public DbSet<LoanApplication> LoanApplications { get; set; } = default!;
    public DbSet<LoanCharge> LoanCharges { get; set; } = default!;
    public DbSet<LoanChargeMapper> LoanChargeMapper { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }


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

        modelBuilder.ApplyConfiguration(new LoanChargeMapConfiguration());

        // Global filters for soft delete
        modelBuilder.Entity<LoanProduct>().Property(p => p.IsDeleted).HasDefaultValue(false);
        modelBuilder.Entity<LoanProduct>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<LoanCharge>().Property(p => p.IsDeleted).HasDefaultValue(false);
        modelBuilder.Entity<LoanCharge>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<LoanCharge>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

    }



}
