using System.ComponentModel.DataAnnotations;

namespace TestTaskAton.Dtos;

public class ChangeUserLoginDto
{
    [Required(ErrorMessage = "Новый логин обязателен.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Логин может содержать латинские буквы, цифры, точки, подчеркивания и дефисы.")]
    public string NewLogin { get; set; } = string.Empty;
}