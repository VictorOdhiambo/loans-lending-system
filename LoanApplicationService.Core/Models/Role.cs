using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LoanApplicationService.Core.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        // Add navigation property for users if needed
    }
} 