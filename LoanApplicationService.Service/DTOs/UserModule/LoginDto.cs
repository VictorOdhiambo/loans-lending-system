using System.ComponentModel.DataAnnotations;

namespace LoanApplicationService.Service.DTOs.UserModule
{
    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
