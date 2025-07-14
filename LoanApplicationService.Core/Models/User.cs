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

        [Required]
        public string Role { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
