using System;
using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Core.Models
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        /// <summary>
        /// User role: SuperAdmin, Admin, Customer, etc.
        /// </summary>
        [Required]
        public string Role { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Indicates if the account is active. Inactive users cannot log in.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
