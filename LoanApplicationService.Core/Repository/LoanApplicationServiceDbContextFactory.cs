using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LoanApplicationService.Core.Repository
{
    // This factory is temporarily disabled to bypass design-time constructor error with IdentityDbContext.
    // public class LoanApplicationServiceDbContextFactory : IDesignTimeDbContextFactory<LoanApplicationServiceDbContext>
    // {
    //     public LoanApplicationServiceDbContext CreateDbContext(string[] args)
    //     {
    //         var optionsBuilder = new DbContextOptionsBuilder<LoanApplicationServiceDbContext>();
    //         optionsBuilder.UseSqlServer("YourConnectionStringHere");
    //         return new LoanApplicationServiceDbContext(optionsBuilder.Options);
    //     }
    // }
} 