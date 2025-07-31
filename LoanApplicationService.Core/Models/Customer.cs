using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Core.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        // Basic Info
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        // Contact
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }

        // New fields
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? NationalId { get; set; }
        public string? EmploymentStatus { get; set; }
        public decimal? AnnualIncome { get; set; }
        public LoanRiskLevel RiskLevel { get; set; }

        // System Linkage
        public Guid UserId { get; set; }
        public required ApplicationUser User { get; set; }

        // Navigation
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
