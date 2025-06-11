using System.ComponentModel.DataAnnotations;
using TestTaskAton.Validations;

namespace TestTaskAton.Dtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "Логин обязателен.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Логин может содержать только латинские буквы и цифры.")]
    public string Login { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Пароль обязателен.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Пароль может содержать только латинские буквы и цифры.")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Имя обязательно.")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Имя может содержать только латинские и русские буквы.")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Пол обязателен.")]
    [RegularExpression(@"^[0-2]$", ErrorMessage = "Пол может содержать только цифры 0-2.")]
    public int Gender { get; set; }
    
    [Required(ErrorMessage = "Дата рождения обязательна.")]
    [ValidBirthday(ErrorMessage = "Дата рождения некорректна.")] 
    public DateTime Birthday { get; set; }
    
    [Required(ErrorMessage = "Флаг для админов обязателен.")]
    public bool IsAdmin { get; set; }


}