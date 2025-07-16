using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationService.Core.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        // Basic Info
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Contact
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // New fields
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? NationalId { get; set; }
        public string? EmploymentStatus { get; set; }
        public decimal? AnnualIncome { get; set; }

        // System Linkage
        public Guid UserId { get; set; }
        public Users User { get; set; }

        // Navigation
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
