using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.DTOs.UserModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LoanApplicationService.Service.Services
{
    public class UserServiceImpl : IUserService
    {
        private readonly LoanApplicationServiceDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserServiceImpl(LoanApplicationServiceDbContext db, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserDTO?> LoginAsync(LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Email))
                return null;
                
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
                return null;
            
            // Password verification is handled by SignInManager in the controller
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> RegisterAsync(UserDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return false;
                
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return false;

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Username,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return false;

            // Assign role if specified
            if (!string.IsNullOrEmpty(dto.RoleName))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, dto.RoleName);
                if (!roleResult.Succeeded) return false;
            }

            return true;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.CurrentPassword) || string.IsNullOrEmpty(dto.NewPassword))
                return false;
                
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return _mapper.Map<List<UserDTO>>(users);
        }

        public async Task<List<UserDTO>> GetAdminsAsync()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            return _mapper.Map<List<UserDTO>>(adminUsers.Where(u => u.IsActive));
        }

        public async Task<bool> UpdateUserRoleAsync(Guid userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove current roles
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return false;
            }

            // Add new role
            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            return addResult.Succeeded;
        }

        public async Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            
            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<List<UserDTO>> GetInactiveUsersAsync()
        {
            var users = await _userManager.Users.Where(u => !u.IsActive).ToListAsync();
            return _mapper.Map<List<UserDTO>>(users);
        }
    }
}
