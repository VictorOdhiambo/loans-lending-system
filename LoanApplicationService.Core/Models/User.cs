using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LoanApplicationService.Core.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// Indicates if the account is active. Inactive users cannot log in.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
  