using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.UserModule
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
