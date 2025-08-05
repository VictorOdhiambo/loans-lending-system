using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.Account
{
    public class AdminChangeCustomerPasswordDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 