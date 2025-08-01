using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.CustomerModule
{
    public class CustomerDto
    {
        public string FullName => $"{FirstName} {LastName}";
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "National ID")]
        public string? NationalId { get; set; }

        [Display(Name = "Employment Status")]
        public string? EmploymentStatus { get; set; }

        [Display(Name = "Annual Income")]
        [DataType(DataType.Currency)]
        public decimal? AnnualIncome { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string? Password { get; set; }

        public LoanApplicationService.CrossCutting.Utils.LoanRiskLevel RiskLevel { get; set; }
    }
}
