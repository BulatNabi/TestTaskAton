using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestTaskAton.Data;
using TestTaskAton.Dtos;
using TestTaskAton.Interfaces;
using TestTaskAton.Models;

namespace TestTaskAton.Repositories
{
    public class UserRepository : IUserInterface
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITokenInterface _tokenService; 

        public UserRepository(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, ITokenInterface tokenService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService; 
        }

        public async Task<User?> GetUserByLoginAsync(string login)
        {
            return await _userManager.FindByNameAsync(login);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<User?> GetUserByLoginOnlyAsync(string login)
        {
            return await _userManager.FindByNameAsync(login);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password, bool isAdmin)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return result;
            }

            if (isAdmin)
            {
                bool adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!adminRoleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Admin", NormalizedName = "ADMIN", Id = Guid.NewGuid() });
                }
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!addToRoleResult.Succeeded)
                {
                    Console.WriteLine($"Ошибка при добавлении пользователя {user.Login} в роль Admin: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }
            }
            return result;
        }

        public async Task<IdentityResult> UpdateUserPropertiesAsync(User user, UpdateUserDto updateDto, string? modifiedBy)
        {
            bool changed = false;
            if (updateDto.Name != null && user.Name != updateDto.Name)
            {
                user.Name = updateDto.Name;
                changed = true;
            }
            if (updateDto.Gender.HasValue && user.Gender != updateDto.Gender.Value)
            {
                user.Gender = updateDto.Gender.Value;
                changed = true;
            }
            if (updateDto.Birthday.HasValue && user.Birthday != updateDto.Birthday.Value)
            {
                user.Birthday = updateDto.Birthday.Value;
                changed = true;
            }

            if (changed)
            {
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = modifiedBy;
                return await _userManager.UpdateAsync(user);
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword, string? modifiedBy)
        {
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ChangeUserLoginAsync(User user, string newLogin, string? modifiedBy)
        {
            var existingUserWithNewLogin = await _userManager.FindByNameAsync(newLogin);
            if (existingUserWithNewLogin != null && existingUserWithNewLogin.Id != user.Id)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Пользователь с таким логином уже существует." });
            }

            user.Login = newLogin;
            user.UserName = newLogin; 
            user.NormalizedUserName = _userManager.NormalizeName(newLogin);

            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> SoftDeleteUserAsync(User user, string? revokedBy)
        {
            if (user.RevokedOn != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Пользователь уже неактивен." });
            }
            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = revokedBy;
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> HardDeleteUserAsync(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> RecoverUserAsync(User user, string? modifiedBy)
        {
            if (user.RevokedOn == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Пользователь уже активен." });
            }
            user.RevokedOn = null;
            user.RevokedBy = null;
            user.LockoutEnabled = false;
            user.LockoutEnd = null;

            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<List<User>> GetActiveUsersSortedByCreatedOnAsync()
        {
            return await _context.Users
                                 .Where(u => u.RevokedOn == null)
                                 .OrderBy(u => u.CreatedOn)
                                 .ToListAsync();
        }

        public async Task<List<User>> GetOlderUsersAsync(int age)
        {
            DateTime now = DateTime.UtcNow;
            DateTime birthDateThreshold = now.AddYears(-age);

            return await _context.Users
                .Where(u => u.Birthday <= birthDateThreshold)
                .ToListAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string login, string password)
        {
            var user = await _userManager.FindByNameAsync(login);
            if (user == null || user.RevokedOn != null)
            {
                return false;
            }
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            return await _tokenService.CreateToken(user); 
        }

        public Task<InfoUserDto> MapUserToInfoUserDto(User user)
        {
            return Task.FromResult(new InfoUserDto
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                IsActive = user.RevokedOn == null
            });
        }
    }
}