using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.UserModule
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
} 