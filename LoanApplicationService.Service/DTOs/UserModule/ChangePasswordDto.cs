using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.UserModule
{
    public class ChangePasswordDto
    {
        public required string Email { get; set; }
        
        [Display(Name = "Current Password")]
        public required string CurrentPassword { get; set; }
        
        [Display(Name = "New Password")]
        public required string NewPassword { get; set; }
        
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
