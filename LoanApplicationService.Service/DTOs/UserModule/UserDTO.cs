namespace LoanApplicationService.Service.DTOs.UserModule
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
