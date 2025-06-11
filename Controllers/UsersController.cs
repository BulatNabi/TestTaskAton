using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TestTaskAton.Dtos;
using TestTaskAton.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; 
using TestTaskAton.Interfaces; 
namespace TestTaskAton.Controllers;

/// <summary>
/// API для управления пользователями.
/// Доступны операции CRUD над сущностью User.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserInterface _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager; 

    public UsersController(IUserInterface userRepository, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    
    private async Task<bool> HasPermissionToModifyUser(Guid targetUserId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return false;

        if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
            return true;

        return currentUser.Id == targetUserId && currentUser.RevokedOn == null;
    }
    /// <summary>
    /// Создает нового пользователя. Доступно только администраторам.
    /// </summary>
    /// <param name="createUserDto">Данные для создания пользователя (логин, пароль, имя, пол, дата рождения, флаг админа).</param>
    /// <returns>Информация о созданном пользователе.</returns>
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingUser = await _userRepository.GetUserByLoginAsync(createUserDto.Login);
        if (existingUser != null)
        {
            return Conflict("Пользователь с таким логином уже существует.");
        }

        var currentUserLogin = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";

        var user = new User
        {
            Login = createUserDto.Login,
            UserName = createUserDto.Login,
            Name = createUserDto.Name,
            Gender = createUserDto.Gender,
            Birthday = createUserDto.Birthday,
            Admin = createUserDto.IsAdmin,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = currentUserLogin,
            ModifiedOn = DateTime.UtcNow,
            ModifiedBy = currentUserLogin
        };

        var result = await _userRepository.CreateUserAsync(user, createUserDto.Password, createUserDto.IsAdmin);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        var infoUserDto = await _userRepository.MapUserToInfoUserDto(user);
        return Ok(infoUserDto);
    }
    /// <summary>
    /// Изменяет имя, пол или дату рождения пользователя.
    /// Может быть изменено администратором или самим пользователем (если он активен).
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <param name="updateDto">Данные для обновления пользователя (имя, пол, дата рождения).</param>
    /// <returns>Обновленная информация о пользователе.</returns>
    [HttpPatch("{id}/update")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var targetUser = await _userRepository.GetUserByIdAsync(id);
        if (targetUser == null)
        {
            return NotFound("Пользователь не найден.");
        }

        if (!await HasPermissionToModifyUser(id))
        {
            return Forbid();
        }

        var currentUserLogin = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _userRepository.UpdateUserPropertiesAsync(targetUser, updateDto, currentUserLogin);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        var infoUserDto = await _userRepository.MapUserToInfoUserDto(targetUser);
        return Ok(infoUserDto);
    }
    /// <summary>
    /// Изменяет пароль пользователя.
    /// Может быть изменено администратором или самим пользователем (если он активен).
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <param name="changePasswordDto">Текущий и новый пароль пользователя.</param>
    /// <returns>Обновленная информация о пользователе.</returns>
    [HttpPatch("{id}/changepassword")]
    [Authorize]
    public async Task<IActionResult> ChangeUserPassword(Guid id, [FromBody] ChangeUserPasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var targetUser = await _userRepository.GetUserByIdAsync(id);
        if (targetUser == null)
        {
            return NotFound("Пользователь не найден.");
        }

        if (!await HasPermissionToModifyUser(id))
        {
            return Forbid();
        }
        
        var currentUserLogin = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _userRepository.ChangeUserPasswordAsync(targetUser, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword, currentUserLogin);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return BadRequest("Неверный текущий пароль.");
            }
            return BadRequest(ModelState);
        }

        var infoUserDto = await _userRepository.MapUserToInfoUserDto(targetUser);
        return Ok(infoUserDto);
    }
    /// <summary>
    /// Изменяет логин пользователя.
    /// Может быть изменено администратором или самим пользователем (если он активен). Логин должен оставаться уникальным.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <param name="changeLoginDto">Новый логин пользователя.</param>
    /// <returns>Обновленная информация о пользователе.</returns>
    [HttpPatch("{id}/changelogin")]
    [Authorize]
    public async Task<IActionResult> ChangeUserLogin(Guid id, [FromBody] ChangeUserLoginDto changeLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var targetUser = await _userRepository.GetUserByIdAsync(id);
        if (targetUser == null)
        {
            return NotFound("Пользователь не найден.");
        }

        if (!await HasPermissionToModifyUser(id))
        {
            return Forbid();
        }
        
        var currentUserLogin = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _userRepository.ChangeUserLoginAsync(targetUser, changeLoginDto.NewLogin, currentUserLogin);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                return Conflict("Пользователь с таким логином уже существует.");
            }
            return BadRequest(ModelState);
        }

        var infoUserDto = await _userRepository.MapUserToInfoUserDto(targetUser);
        return Ok(infoUserDto);
    }
    /// <summary>
    /// Получает список всех активных пользователей, отсортированных по дате создания. Доступно только администраторам.
    /// </summary>
    /// <returns>Список активных пользователей.</returns>
    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<InfoUserDto>>> GetActiveUsers()
    {
        var activeUsers = await _userRepository.GetActiveUsersSortedByCreatedOnAsync();
        var dtos = new List<InfoUserDto>();
        foreach (var user in activeUsers)
        {
            dtos.Add(await _userRepository.MapUserToInfoUserDto(user));
        }
        return Ok(dtos);
    }
    /// <summary>
    /// Получает информацию о пользователе по его логину. Доступно только администраторам.
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <returns>Информация о пользователе (имя, пол, дата рождения, статус активности).</returns>
    [HttpGet("bylogin/{login}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InfoUserDto>> GetUserByLogin([FromRoute] string login)
    {
        var user = await _userRepository.GetUserByLoginAsync(login);
        if (user == null)
        {
            return NotFound("Пользователь не найден.");
        }
        var dto = await _userRepository.MapUserToInfoUserDto(user);
        return Ok(dto);
    }
    /// <summary>
    /// Аутентификация пользователя и выдача JWT токена.
    /// </summary>
    /// <param name="loginDto">Логин и пароль пользователя.</param>
    /// <returns>JWT токен и информация о пользователе.</returns>
    [HttpPost("login")]
    [AllowAnonymous] 
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userRepository.GetUserByLoginAsync(loginDto.Login);
        if (user == null || user.RevokedOn != null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return Unauthorized("Неверный логин или пароль, либо пользователь неактивен.");
        }

        var token = await _userRepository.GenerateJwtTokenAsync(user);
        var userInfo = await _userRepository.MapUserToInfoUserDto(user);

        return Ok(new AuthResponseDto { Token = token, UserInfo = userInfo });
    }
    /// <summary>
    /// Получает список всех пользователей старше указанного возраста. Доступно только администраторам.
    /// </summary>
    /// <param name="age">Минимальный возраст пользователей.</param>
    /// <returns>Список пользователей, соответствующих критерию возраста.</returns>
    [HttpGet("olderthan/{age:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<InfoUserDto>>> GetOlderUsers([FromRoute] int age)
    {
        if (age < 0 || age > 120)
        {
            return BadRequest("Возраст должен быть в диапазоне от 0 до 120.");
        }

        var olderUsers = await _userRepository.GetOlderUsersAsync(age);
        var dtos = new List<InfoUserDto>();
        foreach (var user in olderUsers)
        {
            dtos.Add(await _userRepository.MapUserToInfoUserDto(user));
        }
        return Ok(dtos);
    }
    /// <summary>
    /// Удаляет пользователя по логину (полное удаление или мягкое удаление с простановкой RevokedOn и RevokedBy). Доступно только администраторам.
    /// </summary>
    /// <param name="deleteDto">Логин пользователя и флаг мягкого удаления.</param>
    /// <returns>Статус операции.</returns>
    [HttpDelete("delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserDto deleteDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userToDelete = await _userRepository.GetUserByLoginAsync(deleteDto.Login);
        if (userToDelete == null)
        {
            return NotFound("Пользователь не найден.");
        }

        IdentityResult result;
        if (deleteDto.SoftDelete)
        {
            var currentUserLogin = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            result = await _userRepository.SoftDeleteUserAsync(userToDelete, currentUserLogin);
        }
        else
            result = await _userRepository.HardDeleteUserAsync(userToDelete);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        return Ok("Пользователь успешно удален/деактивирован.");
    }
    /// <summary>
    /// Восстанавливает ранее удаленного (деактивированного) пользователя. Доступно только администраторам.
    /// </summary>
    /// <param name="recoverDto">Логин пользователя для восстановления.</param>
    /// <returns>Обновленная информация о восстановленном пользователе.</returns>
    [HttpPatch("recover")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecoverUser([FromBody] RecoverUserDto recoverDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userToRecover = await _userRepository.GetUserByLoginAsync(recoverDto.Login);
        if (userToRecover == null)
            return NotFound("Пользователь не найден.");

        if (userToRecover.RevokedOn == null)
            return BadRequest("Пользователь уже активен и не нуждается в восстановлении.");

        var currentUserLogin = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _userRepository.RecoverUserAsync(userToRecover, currentUserLogin);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        var infoUserDto = await _userRepository.MapUserToInfoUserDto(userToRecover);
        return Ok(infoUserDto);
    }
}