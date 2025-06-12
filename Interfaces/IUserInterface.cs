using Microsoft.AspNetCore.Identity;
using TestTaskAton.Dtos;
using TestTaskAton.Models;

namespace TestTaskAton.Interfaces;

public interface IUserInterface
{
    Task<User?> GetUserByLoginAsync(string login); 
    Task<User?> GetUserByIdAsync(Guid id); 
    Task<IdentityResult> CreateUserAsync(User user, string password, bool isAdmin);
    Task<IdentityResult> UpdateUserPropertiesAsync(User user, UpdateUserDto updateDto, string modifiedBy);
    Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword, string modifiedBy);
    Task<IdentityResult> ChangeUserLoginAsync(User user, string newLogin, string modifiedBy);
    Task<IdentityResult> SoftDeleteUserAsync(User user, string revokedBy);
    Task<IdentityResult> HardDeleteUserAsync(User user);
    Task<IdentityResult> RecoverUserAsync(User user, string modifiedBy);
    Task<List<User>> GetActiveUsersSortedByCreatedOnAsync();
    Task<List<User>> GetOlderUsersAsync(int age);
    Task<bool> ValidateCredentialsAsync(string login, string password);
    Task<string> GenerateJwtTokenAsync(User user);
    Task<InfoUserDto> MapUserToInfoUserDto(User user);
}