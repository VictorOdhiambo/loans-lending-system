using Loan_application_service.Areas.Identity.Data;
using Loan_application_service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;


namespace Loan_application_service.Data;

public class Loan_application_serviceContext : IdentityDbContext<User>
{
    public Loan_application_serviceContext(DbContextOptions<Loan_application_serviceContext> options)
        : base(options)
    {

    }

    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);




        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
