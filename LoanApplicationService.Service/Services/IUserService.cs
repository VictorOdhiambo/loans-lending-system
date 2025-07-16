using LoanApplicationService.Service.DTOs.UserModule;

namespace LoanApplicationService.Service.Services
{
    public interface IUserService
    {
        Task<UserDTO?> LoginAsync(LoginDto loginDto);
        Task<bool> RegisterAsync(UserDTO dto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<List<UserDTO>> GetAdminsAsync();
    }
}
