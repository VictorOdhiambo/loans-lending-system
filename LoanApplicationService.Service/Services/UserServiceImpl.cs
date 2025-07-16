using AutoMapper;
using LoanApplicationService.Core.Models;
using LoanApplicationService.Core.Repository;
using LoanApplicationService.Service.DTOs.UserModule;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationService.Service.Services
{
    public class UserServiceImpl : IUserService
    {
        private readonly LoanApplicationServiceDbContext _db;
        private readonly IMapper _mapper;

        public UserServiceImpl(LoanApplicationServiceDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<UserDTO?> LoginAsync(LoginDto loginDto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> RegisterAsync(UserDTO dto)
        {
            if (_db.Users.Any(u => u.Email == dto.Email))
                return false;

            var user = _mapper.Map<Users>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _db.Users.Where(u => !u.IsDeleted).ToListAsync();
            return _mapper.Map<List<UserDTO>>(users);
        }

        public async Task<List<UserDTO>> GetAdminsAsync()
        {
            var admins = await _db.Users.Where(u => !u.IsDeleted && u.Role == "Admin").ToListAsync();
            return _mapper.Map<List<UserDTO>>(admins);
        }
    }
}
