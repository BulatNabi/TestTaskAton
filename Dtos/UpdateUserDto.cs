using System.ComponentModel.DataAnnotations;
using TestTaskAton.Validations;

namespace TestTaskAton.Dtos;

public class UpdateUserDto
{
    [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Имя может содержать только латинские и русские буквы.")]
    [StringLength(100, ErrorMessage = "Имя не может быть длиннее 100 символов.")]
    public string? Name { get; set; } 

    [RegularExpression(@"^[0-2]$", ErrorMessage = "Пол может содержать только цифры 0 (не указано), 1 (мужской), 2 (женский).")]
    public int? Gender { get; set; } 

    [ValidBirthday(ErrorMessage = "Дата рождения некорректна.")]
    public DateTime? Birthday { get; set; }
}