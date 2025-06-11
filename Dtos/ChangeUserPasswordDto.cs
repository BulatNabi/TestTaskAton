using System.ComponentModel.DataAnnotations;

namespace TestTaskAton.Dtos;

public class ChangeUserPasswordDto
{
    [Required(ErrorMessage = "Текущий пароль обязателен.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новый пароль обязателен.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Новый пароль может содержать только латинские буквы и цифры.")]
    public string NewPassword { get; set; } = string.Empty;
}