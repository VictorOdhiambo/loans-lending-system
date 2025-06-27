using Loan_application_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loan_application_service.Data
{
    public class LoanChargeConfiguration
    {
        public class LoanChargeMapConfiguration : IEntityTypeConfiguration<LoanChargeMapper>

        {
            public void Configure(EntityTypeBuilder<LoanChargeMapper> builder)
            {
                builder.HasKey(s => new { s.LoanChargeId, s.LoanProductId });
                builder.HasOne(ss => ss.LoanProduct)
                    .WithMany(s => s.LoanChargeMap)
                    .HasForeignKey(ss => ss.LoanChargeId);
                builder.HasOne(ss => ss.LoanCharge)
                    .WithMany(s => s.LoanChargeMap)
                    .HasForeignKey(ss => ss.LoanChargeId);
            }
        }

    }
}
