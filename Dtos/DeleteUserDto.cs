using System.ComponentModel.DataAnnotations;

namespace TestTaskAton.Dtos;

public class DeleteUserDto
{
    [Required(ErrorMessage = "Логин пользователя обязателен.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Тип удаления обязателен.")]
    public bool SoftDelete { get; set; } 
}