using System;
using System.ComponentModel.DataAnnotations;
using LoanApplicationService.CrossCutting.Utils;

namespace LoanApplicationService.Service.DTOs.LoanApplicationModule
{
    public class LoanApplicationDto
    {
        public int ApplicationId { get; set; }


        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ProcessedBy { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Requested amount must be positive.")]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Range(1, 480, ErrorMessage = "Term (months) must be between 1 and 480.")]
        public int TermMonths { get; set; }

        [MaxLength(200)]
        public string? Purpose { get; set; }


        public LoanStatus Status { get; set; } = (int)LoanStatus.Pending;
        public decimal? ApprovedAmount { get; set; }

        public decimal? InterestRate { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public DateTime? DecisionDate { get; set; }

        [MaxLength(500)]
        public string? DecisionNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? ProductName { get; set; }


    }
}
